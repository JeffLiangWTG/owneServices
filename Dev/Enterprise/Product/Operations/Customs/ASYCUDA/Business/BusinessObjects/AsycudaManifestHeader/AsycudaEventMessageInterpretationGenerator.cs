using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public abstract class AsycudaEventMessageInterpretationGenerator
	{
		protected AsycudaEventMessageInterpretationGenerator(UniversalEvent universalEvent)
		{
			this.universalEvent = universalEvent;
		}
		protected UniversalEvent universalEvent;

		protected List<Context> ContextCollection => universalEvent?.ContextCollection;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected abstract List<KeyValuePair<string, string>> GetContentFields();

		public ZString GetInterpretedHTML()
		{
			var tableCreator = new HtmlTableCreator { EnableHTMLEncoding = false };
			tableCreator.WriteRowWithFormatting(
				new CellWithFormatting(
					Res.GetString("5951C670-D635-4CCC-9372-F2548A6C64E5", "Response Details"),
					new NameValueCollection {
						ColspanBold(7),
						TableInterpretation.Attributes.AlignCenter
					}
				)
			);

			WriteDescription(tableCreator);
			WriteContents(tableCreator);

			return tableCreator.ToHtml();
		}

		protected abstract string ActionPurposeCodeDesc { get; }

		void WriteDescription(HtmlTableCreator tableCreator)
		{
			var header = Res.GetString(
				"5F600E2F-89A3-4252-A39E-90C6857F9D24",
				"A response message has been received from Customs.<br />The message type is:  {0} – {1}",
				universalEvent.DataContext.ActionPurposeCode,
				ActionPurposeCodeDesc
			);

			tableCreator.WriteRowWithFormatting(
				new CellWithFormatting(
					header,
					new NameValueCollection {
						Colspan(7),
						TableInterpretation.Attributes.AlignCenter
					}
				)
			);

			var jobNumber = universalEvent.DataContext.DataTargetCollection.FirstOrDefault(
				obj => obj.Type.Equals("AsycudaManifest")
			)?.Key;
			tableCreator.WriteRowWithFormatting(
				new CellWithFormatting("Job Number", ColspanBold(2)),
				new CellWithFormatting(jobNumber, Colspan(5))
			);
		}

		void WriteContents(HtmlTableCreator tableCreator)
		{
			GetContentFields().ForEach(pair => WriteSingleKeyValue(pair, universalEvent.ContextCollection, tableCreator));
			WriteMasterBill(tableCreator);
		}

		void WriteMasterBill(HtmlTableCreator tableCreator)
		{
			var masterBillContext = FindContext("MasterBill", universalEvent.ContextCollection).FirstOrDefault();
			var masterBillNum = masterBillContext?.Value;
			var masterBillStatusCode = FindContextValue("MessageStatusCode", masterBillContext?.SubContextCollection);

			tableCreator.WriteRowWithFormatting(
				new CellWithFormatting("Master Bill", ColspanBold(2)),
				new CellWithFormatting(masterBillNum, Colspan(3)),
				new CellWithFormatting(masterBillStatusCode, Colspan(2))
			);

			WriteHouseBills(masterBillContext, tableCreator);
		}

		protected abstract void WriteConsignmentReference(Context consignmentReference, HtmlTableCreator tableCreator);

		protected abstract void WriteHouseBills(Context masterBill, HtmlTableCreator tableCreator);

		protected void WriteSingleKeyValue(KeyValuePair<string, string> keyValuePair, List<Context> contexts, HtmlTableCreator tableCreator)
		{
			var value = FindContextValue(keyValuePair.Value, contexts);
			tableCreator.WriteRowWithFormatting(
				new CellWithFormatting(keyValuePair.Key, ColspanBold(2)),
				new CellWithFormatting(value, Colspan(5))
			);
		}

		protected string FindContextValue(string type, List<Context> contexts)
		{
			return contexts?.FirstOrDefault(
			context => context.Type?.Type?.ToString() == type
			)?.Value;
		}

		protected IEnumerable<Context> FindContext(string type, List<Context> contexts)
		{
			return contexts?.Where(subContext => subContext.Type?.Type?.ToString() == type) ?? Enumerable.Empty<Context>();
		}

		protected NameValueCollection Colspan(int span)
		{
			return TableInterpretation.Attributes.GetColspanAttribute(span);
		}

		protected NameValueCollection ColspanBold(int span)
		{
			return new NameValueCollection
			{
				Colspan(span),
				new NameValueCollection { { "font-weight", "bold" } }
			};
		}
	}
}
