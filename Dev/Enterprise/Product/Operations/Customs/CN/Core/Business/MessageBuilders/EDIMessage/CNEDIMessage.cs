using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CN.Business
{
	public class CNEDIMessage : EDIMessage, Integration.Customs.CN.IEDIMessage
	{
		public CNEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override string GetMessageReferenceNumber()
		{
			return Environment.Env.NumberFountains.EDIFACTNumberFountain("M", GlbCompany.CurrentCompany.LicenceKeyIdentifier, ApplicationCodeList.Codes.CNCustomsSingleWindow).GetNextFormatted(Factory).PadLeft(20, '0');
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodeList.Codes.CNCustomsSingleWindow;
		}
	}
}
