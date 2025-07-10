using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBRDHSAmendmentDetailsManager : FTAAmendmentDetailsManager<ImportDHRHeader, IImportDHRHeader>
	{
		public GOVCBRDHSAmendmentDetailsManager(CusEntryHeader entry, ZString messageType)
			: base(entry, messageType)
		{
		}

		protected override ImportDHRHeader GetCurrentDataProvider() => new ImportDHRCreator().Create(Entry);

		public ZString AmendmentTypeForInvoiceLine
		{
			get
			{
				if (!amendmentTypeForInvoiceLine.HasValue)
				{
					amendmentTypeForInvoiceLine = DHSAmendmentTypeForInvoiceLine.Codes.CXX;
					var amendTypeSum = CalculateAmendmentType(nameof(IImportDHRInvoiceLine));
					switch (amendTypeSum)
					{
						case 1:
							amendmentTypeForInvoiceLine = DHSAmendmentTypeForInvoiceLine.Codes.XXA;
							break;
						case 2:
							amendmentTypeForInvoiceLine = DHSAmendmentTypeForInvoiceLine.Codes.XRX;
							break;
						case 3:
							amendmentTypeForInvoiceLine = DHSAmendmentTypeForInvoiceLine.Codes.XRA;
							break;
						case 4:
							amendmentTypeForInvoiceLine = DHSAmendmentTypeForInvoiceLine.Codes.CXX;
							break;
						case 5:
							amendmentTypeForInvoiceLine = DHSAmendmentTypeForInvoiceLine.Codes.CXA;
							break;
						case 6:
							amendmentTypeForInvoiceLine = DHSAmendmentTypeForInvoiceLine.Codes.CRX;
							break;
						case 7:
							amendmentTypeForInvoiceLine = DHSAmendmentTypeForInvoiceLine.Codes.CRA;
							break;
					}
				}
				return amendmentTypeForInvoiceLine.Value;
			}
		}
		ZString? amendmentTypeForInvoiceLine;
	}
}
