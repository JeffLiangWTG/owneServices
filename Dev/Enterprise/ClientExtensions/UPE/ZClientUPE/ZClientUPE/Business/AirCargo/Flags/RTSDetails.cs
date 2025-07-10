using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class RTSDetails : RTSTranshipmentDetails
	{
		public RTSDetails(UPECusHAWB uPECusHAWB)
			: base(uPECusHAWB)
		{
		}

		public override string FlagName
		{
			get { return "RTS"; }
		}

		#region Default Values

		protected override ZString DefaultAddressName
		{
			get { return "Consignor"; }
		}

		protected override ZString DefaultName
		{
			get { return UPECusHAWB.CS_ConsignorName; }
		}

		protected override ZString DefaultStreet
		{
			get { return UPECusHAWB.CS_ConsignorStreet; }
		}

		protected override ZString DefaultStreet2
		{
			get { return UPECusHAWB.CS_ConsignorStreet2; }
		}

		protected override ZString DefaultCity
		{
			get { return UPECusHAWB.CS_ConsignorCity; }
		}

		protected override ZString DefaultState
		{
			get { return UPECusHAWB.CS_ConsignorState; }
		}

		protected override ZString DefaultPostCode
		{
			get { return UPECusHAWB.CS_ConsignorPostcode; }
		}

		protected override ZString DefaultCountry
		{
			get { return UPECusHAWB.CS_RN_NKConsignorCountry; }
		}

		#endregion

		#region MaxLengths

		protected override int Name_MaxLength
		{
			get { return CusHAWBSchema.CS_ConsignorName.MaxLength; }
		}

		protected override int Street_MaxLength
		{
			get { return CusHAWBSchema.CS_ConsignorStreet.MaxLength; }
		}

		protected override int Street2_MaxLength
		{
			get { return CusHAWBSchema.CS_ConsignorStreet2.MaxLength; }
		}

		protected override int City_MaxLength
		{
			get { return CusHAWBSchema.CS_ConsignorCity.MaxLength; }
		}

		protected override int State_MaxLength
		{
			get { return CusHAWBSchema.CS_ConsignorState.MaxLength; }
		}

		protected override int PostCode_MaxLength
		{
			get { return CusHAWBSchema.CS_ConsignorPostcode.MaxLength; }
		}

		protected override int Country_MaxLength
		{
			get { return CusHAWBSchema.CS_RN_NKConsignorCountry.MaxLength; }
		}

		#endregion
	}
}
