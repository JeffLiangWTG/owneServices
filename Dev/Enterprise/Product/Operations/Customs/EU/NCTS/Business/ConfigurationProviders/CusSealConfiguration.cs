namespace Enterprise.Customs.EU.NCTS.Business;

public class CusSealConfiguration
{
	public ICusSealValidationDecider GetValidationDecider(NctsHeader header) => GetValidationDeciderCore(header);

	protected virtual ICusSealValidationDecider GetValidationDeciderCore(NctsHeader header)
	{
		if (header?.IsPhase5 ?? false)
		{
			return GetCusSealPhase5ValidationDecider();
		}
		else if (header is not null)
		{
			return GetCusSealValidationDecider();
		}

		return null;
	}

	protected virtual ICusSealValidationDecider GetCusSealValidationDecider() => new CusSealValidationDecider();

	protected virtual ICusSealPhase5ValidationDecider GetCusSealPhase5ValidationDecider() => new CusSealPhase5ValidationDecider();
}
