using CargoWise.Types;

namespace Enterprise.DocumentEngine.Business.Validation
{
	internal static class SystemClientPivotValidator
	{
		public static ZString GetErrorText(ZBool parentIsSystemDefined, ZBool parentIsClientSpecific, ZBool childIsSystemDefined, ZBool childIsClientSpecific, ZBool isSystemDefined, ZBool isClientSpecific)
		{
			ZString result = ZString.Empty;
			if (isSystemDefined && !isClientSpecific)
			{
				if (parentIsClientSpecific)
				{
					result = Res.GetString("ddc3ef92-071b-4d56-adde-827f6ae9b823", "System defined joining relationship cannot be added to a client specific document / report.");
				}
				else if (!parentIsSystemDefined)
				{
					result = Res.GetString("931f6553-c9bc-4171-9d4c-7c12eefd24de", "System defined joining relationship cannot be added to a user defined document / report.");
				}

				if (childIsClientSpecific)
				{
					result = Res.GetString("d84df00d-0ca5-47a9-b703-4020b1273918", "System defined joining relationship cannot use a client specific template / document.");
				}
				else if (!childIsSystemDefined)
				{
					result = Res.GetString("f14e9b8d-2486-4cbd-8ccd-e4426aa39040", "System defined joining relationship cannot use a user defined template / document.");
				}
			}
			else if (isSystemDefined && isClientSpecific)
			{
				if (!parentIsSystemDefined)
				{
					result = Res.GetString("a2979d49-28fa-498f-8a0c-21127195f058", "Client specific joining relationship cannot be added to a user defined document / report.");
				}

				if (!childIsSystemDefined)
				{
					result = Res.GetString("e0cc1a64-3a71-4736-9353-30c9fd6ca7f0", "Client specific joining relationship cannot use user defined template / document.");
				}
			}
			else if (!isSystemDefined && !isClientSpecific)
			{
				if (parentIsClientSpecific)
				{
					result = Res.GetString("3024480a-bc85-4993-9a96-ab9b78a25337", "User defined joining relationship cannot be added to a client specific document / report.");
				}
				else if (parentIsSystemDefined)
				{
					result = Res.GetString("eb60c142-5c38-4be4-8d1a-1fb6c19242a1", "User defined joining relationship cannot be added to a system defined document / report.");
				}
			}
			return result;
		}
	}
}
