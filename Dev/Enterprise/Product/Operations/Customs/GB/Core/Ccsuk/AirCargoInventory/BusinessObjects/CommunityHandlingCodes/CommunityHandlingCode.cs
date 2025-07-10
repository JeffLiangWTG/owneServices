using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.GbCcsukSpecialHandling)]
	public class CommunityHandlingCode : AutoCcsukCusAddInfo
	{
		public CommunityHandlingCode(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
			this.hawbParentOfAddInfo = (CusHAWB)((CusAddInfo)addInfoProperty.BizObj).Parent;  //yuk
		}

		[LightValidationTestExempt]
		[List(nameof(Lookups) + "." + nameof(CcsukCusAddInfoLookups.SpecialHandlingCodes))]
		public override ZString C4_CommunityHandlingCode
		{
			get { return base.C4_CommunityHandlingCode; }
			set { base.C4_CommunityHandlingCode = value; }
		}

		public ZString SpecialHandlingCodeDescription
		{
			get { return Lookups.SpecialHandlingCodes.GetDescriptionFromCode(C4_CommunityHandlingCode); }
		}

		[List(nameof(Awb) + "." + nameof(ICcsukCusAwb.Splits))]
		[ReadOnlyMember(nameof(C4_SplitReferenceToWhichThisPertainsReadOnly))]
		public override ZString C4_SplitReferenceToWhichThisPertains
		{
			get { return base.C4_SplitReferenceToWhichThisPertains; }
			set { base.C4_SplitReferenceToWhichThisPertains = value; }
		}

		bool C4_SplitReferenceToWhichThisPertainsReadOnly
		{
			get { return Awb == null || !Awb.HasSplits; }
		}

		public //for list binding, pfff
		ICcsukCusAwb Awb
		{
			get
			{
				ICcsukCusAwb result = null;
				if (hawbParentOfAddInfo != null)
				{
					if (hawbParentOfAddInfo.CS_IsMasterHouse)
					{
						result = hawbParentOfAddInfo.MAWB; // for basics
					}
					else
					{
						result = hawbParentOfAddInfo;
					}
				}
				return result;
			}
		}
		readonly CusHAWB hawbParentOfAddInfo;
	}
}
