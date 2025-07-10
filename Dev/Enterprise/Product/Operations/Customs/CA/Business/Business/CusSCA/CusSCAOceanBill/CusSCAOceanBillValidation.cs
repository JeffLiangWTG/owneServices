using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusSCAOceanBillValidation : Customs.Business.CusSCAOceanBillValidation
	{
		public CusSCAOceanBillValidation(Customs.Business.BaseCusSCAOceanBill parent)
			: base(parent)
		{
		}

		public CusSCAOceanBill OcealBill
		{
			get { return Parent; }
		}

		protected new CusSCAOceanBill Parent
		{
			get { return (CusSCAOceanBill)base.Parent; }
		}

		public void ValidateOriginalCCN()
		{
			ValidateCalculatedProperty(OcealBill.OriginalCCNInfo);
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateOriginalCCN();
		}

		protected void CheckOriginalCCN()
		{
			if (OcealBill.OriginalCCN.Length < 5)
			{
				OcealBill.OriginalCCNInfo.AddMessageError(CusSCAHouseValidation.OrigCCNMessage);
			}
		}

		protected override void CheckCB_OceanBill()
		{
			base.CheckCB_OceanBill();
			MandatoryValidation.MessageErrorIfNotEntered(OcealBill.CB_OceanBillInfo, Res.GetString("e6c0290d-9cde-4f0f-b9ee-d27699eb55b4", "Master Bill"));
			if (OcealBill.CB_OceanBill.Length > 30)
			{
				OcealBill.CB_OceanBillInfo.AddMessageError(Res.GetString("A391C2D0-C7C7-4055-97FA-9719153B057C", "The master bill number is too long for the message. It will result in a syntax error if you proceed. The maximum length allowed is 30."));
			}
		}
	}
}
