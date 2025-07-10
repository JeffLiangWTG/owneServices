using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.EU.NCTS.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.EuNctsUnloadingRemark)]
	public class UnloadingRemarkAddInfo : AutoUnloadingRemarkAddInfo
	{
		public UnloadingRemarkAddInfo(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public UnloadingRemarkAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(UnloadingRemarkAddInfoLookups.YesNoCodeList))]
		[ResourceStringData("UnloadingRemarkAddInfo.Conform", Caption = "Unloading Conforms?")]
		public override ZString G9_Conform
		{
			get => base.G9_Conform;
			set => base.G9_Conform = value;
		}

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(UnloadingRemarkAddInfoLookups.YesNoEmptyCodeList))]
		[ResourceStringData("UnloadingRemarkAddInfo.StateOfSealsOk", Caption = "State of Seals OK?", ShortCaption = "Seals OK?")]
		public override ZString G9_StateOfSealsOk
		{
			get => base.G9_StateOfSealsOk;
			set => base.G9_StateOfSealsOk = value;
		}

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(UnloadingRemarkAddInfoLookups.YesNoCodeList))]
		[ResourceStringData("UnloadingRemarkAddInfo.UnloadingCompletion", Caption = "Unloading Completed?", ShortCaption = "Completed?")]
		public override ZString G9_UnloadingCompletion
		{
			get => base.G9_UnloadingCompletion;
			set => base.G9_UnloadingCompletion = value;
		}

		[ReadOnlyMember(nameof(G9_NoOfSeals_ReadOnly))]
		[ResourceStringData("2EFEE087-1E83-40A4-B955-EC53F23B7B06", Caption = "No. of Seals")]
		public override ZInt G9_NoOfSeals
		{
			get => base.G9_NoOfSeals;
			set => base.G9_NoOfSeals = value;
		}

		public bool G9_NoOfSeals_ReadOnly => G9_StateOfSealsOk != YesNoList.Codes.No;

		public new CusAddInfo<UnloadingRemarkAddInfo> Parent => (CusAddInfo<UnloadingRemarkAddInfo>)base.Parent;

		public NctsHeader NctsHeader => Parent.Parent as NctsHeader;
	}
}
