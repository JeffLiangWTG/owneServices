using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.DocumentWrappers
{
	public class DocSADHLine : Enterprise.DocumentWrappers.Customs.EU.DocSADHLine
	{
		protected DocSADHLine(EU.Business.Declaration.CusEntryLine entryLine, BusinessObjectFactory factory)
			: base(entryLine, factory)
		{
		}

		public static new DocSADHLine New(EU.Business.Declaration.CusEntryLine entryLine, BusinessObjectFactory factory)
		{
			return entryLine == null ? null : new DocSADHLine(entryLine, factory);
		}

		protected override void AddSupportingDocumentsToFirstPage(ZStringBuilder result)
		{
			var mucr = (Declaration as JobDeclaration)?.JE_MasterUCR ?? ZString.Empty;
			if (!mucr.IsEmpty)
			{
				result.Append("9MCR-" + mucr);
			}
		}

		public override ZString Box48DeferredPayment
		{
			get
			{
				var result = new ZStringBuilder();
				if (!Declaration.JE_PaymentMethod.IsEmpty)
				{
					result.Append(Declaration.JE_PaymentMethod + " " + Declaration.JE_DefermentAccountNumber);
				}
				if (!Declaration.ZG_VATDeferType.IsEmpty)
				{
					result.Append(Declaration.ZG_VATDeferType + " " + Declaration.ZG_VATDeferNumber);
				}
				return result.ToStringWithDelimiterBetweenAppends("; ");
			}
		}

		protected override Enterprise.DocumentWrappers.Customs.EU.ProducedDocumentsCertificatesBuilder GetProducedDocumentsCertificatesBuilder() =>
			new ProducedDocumentsCertificatesBuilder();
	}
}
