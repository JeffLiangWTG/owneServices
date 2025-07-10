using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class TranshipmentDetails : RTSTranshipmentDetails
	{
		public TranshipmentDetails(UPECusHAWB uPECusHAWB)
			: base(uPECusHAWB)
		{
		}

		public override string FlagName
		{
			get { return "Transhipment"; }
		}

		#region Default Values

		protected override ZString DefaultAddressName
		{
			get { return "Consignee"; }
		}

		protected override ZString DefaultName
		{
			get { return UPECusHAWB.CS_ConsigneeName; }
		}

		protected override ZString DefaultStreet
		{
			get { return UPECusHAWB.CS_ConsigneeStreet; }
		}

		protected override ZString DefaultStreet2
		{
			get { return UPECusHAWB.CS_ConsigneeStreet2; }
		}

		protected override ZString DefaultCity
		{
			get { return UPECusHAWB.CS_ConsigneeCity; }
		}

		protected override ZString DefaultState
		{
			get { return UPECusHAWB.CS_ConsigneeState; }
		}

		protected override ZString DefaultPostCode
		{
			get { return UPECusHAWB.CS_ConsigneePostcode; }
		}

		protected override ZString DefaultCountry
		{
			get { return UPECusHAWB.CS_RN_NKConsigneeCountry; }
		}

		#endregion

		#region MaxLengths

		protected override int Name_MaxLength
		{
			get { return CusHAWBSchema.CS_ConsigneeName.MaxLength; }
		}

		protected override int Street_MaxLength
		{
			get { return CusHAWBSchema.CS_ConsigneeStreet.MaxLength; }
		}

		protected override int Street2_MaxLength
		{
			get { return CusHAWBSchema.CS_ConsigneeStreet2.MaxLength; }
		}

		protected override int City_MaxLength
		{
			get { return CusHAWBSchema.CS_ConsigneeCity.MaxLength; }
		}

		protected override int State_MaxLength
		{
			get { return CusHAWBSchema.CS_ConsigneeState.MaxLength; }
		}

		protected override int PostCode_MaxLength
		{
			get { return CusHAWBSchema.CS_ConsigneePostcode.MaxLength; }
		}

		protected override int Country_MaxLength
		{
			get { return CusHAWBSchema.CS_RN_NKConsigneeCountry.MaxLength; }
		}

		#endregion
	}
}
