using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business
{
	public class TSTCustomsNumberViewStmNumsWrapper : CustomsNumberViewStmNumsWrapper
	{
		public TSTCustomsNumberViewStmNumsWrapper(CustomsNumberViewStmNums stmNums) : base(stmNums)
		{
		}

		new class Schema : CustomsNumberViewStmNumsWrapper.Schema
		{
			public const int SN_FountainNameMaxLength = 10;
		}

		[MaxLength(Schema.SN_FountainNameMaxLength)]
		[ResourceStringData("TSTCustomsNumberViewStmNumsWrapper|SN_FountainName", Caption = "Prefix")]
		public override ZString SN_FountainName
		{
			get => base.SN_FountainName;
			set => base.SN_FountainName = value;
		}
	}
}
