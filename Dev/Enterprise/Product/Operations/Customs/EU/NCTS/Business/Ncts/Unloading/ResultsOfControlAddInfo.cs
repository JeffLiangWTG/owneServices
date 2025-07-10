using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.EU.NCTS.Business
{
	[CusAddInfoType(CusAddInfoTypeAttribute.Codes.EuNctsResultsOfControl)]
	public class ResultsOfControlAddInfo : AutoResultsOfControlAddInfo
	{
		public ResultsOfControlAddInfo(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ResultsOfControlAddInfo(ZPropertyInfo addInfoProperty)
			: base(addInfoProperty.BizObj.Factory)
		{
			SetupEventsAndLoadValues(addInfoProperty);
		}

		public NctsHeader ArrivalParent
		{
			get { return ((CusAddInfo<ResultsOfControlAddInfo>)base.Parent).Parent as NctsHeader; }
		}

		internal bool IsNationalityAtDept
		{
			get { return G9_PointerToTheAttribute == NctsHeader.MeansOfTransportAtDepartureNationalityPointer; }
		}

		public override ZString G9_ControlIndicator
		{
			get => base.G9_ControlIndicator;
			set
			{
				base.G9_ControlIndicator = value;
				ArrivalParent?.MarkAsNeedingValidation();
			}
		}

		public override ZString G9_CorrectedValue
		{
			get => base.G9_CorrectedValue;
			set
			{
				base.G9_CorrectedValue = value;
				ArrivalParent?.MarkAsNeedingValidation();
			}
		}

		public override ZString G9_Description
		{
			get => base.G9_Description;
			set
			{
				base.G9_Description = value;
				ArrivalParent?.MarkAsNeedingValidation();
			}
		}
		public override ZString G9_PointerToTheAttribute
		{
			get => base.G9_PointerToTheAttribute;
			set
			{
				base.G9_PointerToTheAttribute = value;
				ArrivalParent?.MarkAsNeedingValidation();
			}
		}
	}
}
