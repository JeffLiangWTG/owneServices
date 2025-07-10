using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.GB.Business.Declaration
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.GBMaritimeUCNThatIsHeld)]
	public class MaritimeUcnThatIsHeld : AutoMaritimeUcnThatIsHeld
	{
		public MaritimeUcnThatIsHeld(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupParentAndEventsAndLoadValues(addInfoProperty);  // if this fails to be recognised, check that AutoMaritimeUcnThatIsHeld descends from ImportExportAwareAddInfo instead of EnterpriseBusinessObject
		}

		public override ZString KeyToDeterimeUniqueness
		{
			get { return NW_UCN; }
		}
	}
}
