using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public interface IAttributeCusCodeDataParent
	{
		AttributeCusCodeDataCollection GetAttributes(string type);
		ZDateTime EffectiveAssessmentDate { get; }
	}
}
