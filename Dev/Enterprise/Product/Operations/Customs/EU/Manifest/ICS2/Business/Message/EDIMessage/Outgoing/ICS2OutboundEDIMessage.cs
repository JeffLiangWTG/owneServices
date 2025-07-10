using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class ICS2OutboundEDIMessage : EDIMessage
	{
		public ICS2OutboundEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		const string MessageReferenceNumberPreifx = "ICS2";

		protected override string GetMessageReferenceNumber() => Env.NumberFountains.EUMessageControlNumber(MessageReferenceNumberPreifx).GetNextFormatted(Factory);

		protected override EDIMessageLookups GetNewLookups() => new ICS2OutboundEDIMessageLookups(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.IC2;
			EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		}

		protected override IEnumerable<Type> GetAdditionalRegisteredLinkedObjectTypes()
		{
			yield return typeof(AsycudaManifestHeader);
		}

		public override bool UsesPlaceHolders => true;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string LRNPlaceHolder = "<<LRN PLACE HOLDER>>";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string LRNPlaceHolderHtml = "&lt;&lt;LRN PLACE HOLDER&gt;&gt;";

		protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
		{
			base.GetNumberFountainNumbersAndFillInPlaceHolders();
			ReplaceMessageTextPlaceHolders();
			SetMessageInterpretation();
		}

		protected virtual void ReplaceMessageTextPlaceHolders()
		{
			var result = EM_MessageText;

			if (result.IndexOf(LRNPlaceHolderHtml) != -1)
			{
				result = result.Replace(LRNPlaceHolderHtml, GetNewLocalReferenceNumber());
			}

			EM_MessageText = result;
		}

		void SetMessageInterpretation()
		{
			EM_MessageInterpretation = ConvertXmlToHtml(EM_MessageText);
		}

		string GetNewLocalReferenceNumber()
		{
			var result = ZString.Empty;
			if (EM_LinkTable == AsycudaManifestHeader.Schema.TableName)
			{
				var header = EM_LinkedObject as AsycudaManifestHeader;
				if (header != null)
				{
					header.GenerateNewLocalReferenceNumber();
					result = header.LocalReferenceNumber;
				}
			}
			return result;
		}
	}
}
