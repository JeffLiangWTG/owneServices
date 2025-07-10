using CargoWise.Types;

namespace Enterprise.Customs.EU.Business;

public class ErrorInfo
{
	public ErrorInfo(ZString box, ZString requirementLevel)
	{
		this.Box = box;
		this.RequirementLevel = requirementLevel;
	}

	public ErrorInfo(ZString box, ZString requirementLevel, bool useDefaultIntroduction)
	{
		this.Box = box;
		this.RequirementLevel = requirementLevel;
		this.useDefaultIntroduction = useDefaultIntroduction;
	}

	public ZString Introduction { get { return useDefaultIntroduction ? Res.GetString("d46b65e2-5bc9-4db6-8aa4-054d4c58b77e", "Missing data for {0} field: ", RequirementLevel) : ""; } }

	public readonly ZString Box;
	public readonly ZString RequirementLevel;
	readonly bool useDefaultIntroduction = true;
}
