using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Netting;
using static Enterprise.Accounting.Business.AccountingConstants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.DataTransfer.Universal.Netting
{
	public static class HelperMethods
	{
		public static ZBool HasReferenceValue(this NettingReceivableTransactionReferenceCollection references, ZString referenceType)
		{
			return references?.Cast<NettingReceivableTransactionRef>().Any(x => x.NRR_Type == referenceType) ?? false;
		}

		public static ZString GetReferenceValue(this NettingReceivableTransactionReferenceCollection references, ZString referenceType)
		{
			return references != null ? references.Cast<NettingReceivableTransactionRef>().FirstOrDefault(x => x.NRR_Type == referenceType).NRR_Reference : ZString.Empty;
		}

		public static ZString[] GetReferenceValues(this NettingReceivableTransactionReferenceCollection references, ZString referenceType)
		{
			return references != null ? references.Cast<NettingReceivableTransactionRef>().Where(x => x.NRR_Type == referenceType).Select(result => result.NRR_Reference).ToArray() : Array.Empty<ZString>();
		}

		public static ZBool HasReferenceValue(this NettingReceivableLineReferenceCollection references, ZString referenceType)
		{
			return references?.Cast<NettingReceivableLineReference>().Any(x => x.NR1_Type == referenceType) ?? false;
		}

		public static ZString GetReferenceValue(this NettingReceivableLineReferenceCollection references, ZString referenceType)
		{
			return references != null ? references.Cast<NettingReceivableLineReference>().FirstOrDefault(x => x.NR1_Type == referenceType).NR1_Reference : ZString.Empty;
		}

		public static ZString[] GetReferenceValues(this NettingReceivableLineReferenceCollection references, ZString referenceType)
		{
			return references != null ? references.Cast<NettingReceivableLineReference>().Where(x => x.NR1_Type == referenceType).Select(result => result.NR1_Reference).ToArray() : Array.Empty<ZString>();
		}

		public static ZString GetStatus(ZString reference)
		{
			var index = reference.LastIndexOf(InvoiceAdditionalReference.Separator, StringComparison.Ordinal);
			return index >= 0 ? reference.Substring(index + 1) : ZString.Empty;
		}

		public static string GetValueFromContextCollection(UniversalEvent xmlEvent, ZString type, bool throwExceptionWhenEmpty = false)
		{
			string result;
			result = xmlEvent.ContextCollection?.Where(x => (x.Type?.Type.GetValueOrDefault() ?? ZString.Empty) == type)
				.Select(o => o.Value.GetValueOrDefault()).FirstOrDefault();

			if (throwExceptionWhenEmpty && string.IsNullOrWhiteSpace(result))
			{
				throw new IncorrectDataSetupException(Res.GetString("5A51B90A-0FC2-4BF3-8C69-2416231BB37F", "'{0}' is empty.", type));
			}

			return result;
		}
	}
}
