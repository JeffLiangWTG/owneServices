using System;

namespace Enterprise.GraphEngine.ServiceTasks
{
	public enum GrEngineServiceSetting
	{
		Disabled,
		Worker,
		KeyGen,
		Flipper,
		NonGrengineOnly,
	}

	public static class GrEngineServiceSetting_Extensions
	{
		public static bool IsWorker(this GrEngineServiceSetting setting)
		{
			switch (setting)
			{
				case GrEngineServiceSetting.Disabled:
				case GrEngineServiceSetting.Flipper:
				case GrEngineServiceSetting.NonGrengineOnly:
					return false;

				case GrEngineServiceSetting.Worker:
				case GrEngineServiceSetting.KeyGen:
					return true;

				default:
					throw new NotImplementedException("Is this a worker setting?");
			}
		}
	}
}
