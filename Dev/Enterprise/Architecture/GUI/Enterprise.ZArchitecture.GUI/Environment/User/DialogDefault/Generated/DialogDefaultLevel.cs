using System;
using System.Collections.ObjectModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Core.DialogDefault
{
	public partial class DialogDefaultLevel
	{
		[ThreadStatic]
		static ReadOnlyCollection<string> _levelCodesInOrderOfInclusivity;
		public static ReadOnlyCollection<string> LevelCodesInOrderOfInclusivity
		{
			get
			{
				return _levelCodesInOrderOfInclusivity ??
					(_levelCodesInOrderOfInclusivity = new ReadOnlyCollection<string>(
						new[] { Codes.User, Codes.Company, Codes.Global }
					));
			}
		}

		public static CodeDescriptionPairList DialogDefaultLevelsForCurrentUser
		{
			get
			{
				var list = new CodeDescriptionPairList();
				list.AddPair(Codes.User, Descriptions.User);

				if (CanCreateGlobalDialogDefaults.IsAllowed)
				{
					list.AddPair(Codes.Company, Descriptions.Company);
					list.AddPair(Codes.Global, Descriptions.Global);
				}

				return list;
			}
		}

		static ISecurityCheckpoint CanCreateGlobalDialogDefaults
		{
			get { return EnvProxy.Instance.Security.FindCheckPoint("CanCreateAndModifyGlobalDialogDefaults"); }
		}
	}
}
