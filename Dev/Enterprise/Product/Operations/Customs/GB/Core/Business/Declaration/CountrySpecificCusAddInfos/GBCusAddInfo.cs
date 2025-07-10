using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Declaration
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.GBAllSimpleProperties)]
	public class GBCusAddInfo : AutoGBCusAddInfo
	{
		public GBCusAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupParentAndEventsAndLoadValues(addInfoProperty);
		}

		public JobDeclaration Declaration { get; set; }

		[LightValidationTestExempt]
		public override ZString G9_RouteOfEntry
		{
			get { return base.G9_RouteOfEntry; }
			set { base.G9_RouteOfEntry = value; }
		}

		[LightValidationTestExempt]
		public override ZString G9_IrcInventoryReturnCode
		{
			get { return base.G9_IrcInventoryReturnCode; }
			set { base.G9_IrcInventoryReturnCode = value; }
		}

		[List(nameof(GbNch1PriorityList))]
		public override ZString G9_Nch1Priority
		{
			get { return base.G9_Nch1Priority; }
			set { base.G9_Nch1Priority = value; }
		}

		[List(nameof(GbNch1RequestTypeList))]
		public override ZString G9_Nch1RequestType
		{
			get { return base.G9_Nch1RequestType; }
			set { base.G9_Nch1RequestType = value; }
		}

		public override ZString KeyToDeterimeUniqueness
		{
			get { return G9_RouteOfEntry; }
		}

		public CodeDescriptionPairList GbNch1PriorityList
		{
			get { return Factory.GetCachedValue<Nch1PriorityTypes>(); }
		}

		public CodeDescriptionPairList GbNch1RequestTypeList
		{
			get { return Factory.GetCachedValue<Nch1RequestTypes>(); }
		}
	}
}
