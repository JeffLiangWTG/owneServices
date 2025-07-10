using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.DataTransfer.Native.Utils.Models;

namespace Enterprise.DataTransfer.Native.Business.Update
{
	class OrgContactInterceptor : BaseInterceptor
	{
		public OrgContactInterceptor(IInterceptorSetting setting, AncillaryImportServices sessionServices)
			   : base(setting, sessionServices)
		{
		}

		public override void Invoke(IEntitySet entitySet)
		{
			entitySet.Root.DepthFirstTraversal((entity, relative) => { UpdateContactGender(entity); });

			Function(entitySet);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		void UpdateContactGender(IEntity root)
		{
			if (root.TableName == "OrgContact")
			{
				if (root.HasProperty("Gender"))
				{
					var gender = root["Gender"].ToString();
					if (string.IsNullOrEmpty(gender) || gender.Length > 1)
					{
						root["Gender"] = "N";
					}
				}
			}
		}
	}
}
