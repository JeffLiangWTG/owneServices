using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.DataTransfer.Native.Utils.Models;

namespace Enterprise.DataTransfer.Native.Business.Update.AddInfoOrgMatching
{
	public class AddInfoOrgMatchingInterceptor : MIDOrgMatchingInterceptor
	{
		public AddInfoOrgMatchingInterceptor(IInterceptorSetting setting, AncillaryImportServices sessionServices)
			: base(setting, sessionServices)
		{ }

		public AddInfoOrgMatchingSetting Setting
		{
			get { return (AddInfoOrgMatchingSetting)InterceptorSetting; }
		}

		public override void Invoke(IEntitySet entitySet)
		{
			entitySet.Root.DepthFirstTraversal((entity, relative) => { MatchAddInfoOrgByMID(entity); });

			Function(entitySet);
		}

		public void MatchAddInfoOrgByMID(IEntity entity)
		{
			MatchAddInfoOrgByMID(entity, XmlConstants.PropertyNames.AddInfo);
			MatchAddInfoOrgByMID(entity, XmlConstants.PropertyNames.NAddInfo);
			MatchAddInfoOrgByMID(entity, XmlConstants.PropertyNames.AddInfoData);
			MatchAddInfoOrgByMID(entity, XmlConstants.PropertyNames.NAddInfoData);
		}

		void MatchAddInfoOrgByMID(IEntity entity, string propertyName)
		{
			if (entity.HasProperty(propertyName))
			{
				var addInfos = AddInfoParser.CreateDictionaryWithAddInfoString(entity[propertyName].ToString());
				if (addInfos.Count > 0)
				{
					foreach (var addInfo in addInfos.ToArray())
					{
						var key = addInfo.Key;
						if (key.IsEmpty)
						{
							addInfos.Remove(key);
						}
						else
						{
							var value = addInfo.Value;

							if (key.ToString().IsAddInfoAddress())
							{
								if (value.IsEmpty)
								{
									addInfos.Remove(key);
								}
								else if (!ZGuid.TryParse(value, out _))
								{
									var orgAddress = TryCreateMIDOrganization(value);
									if (orgAddress != null)
									{
										addInfos[key] = orgAddress.PK.ToString();
									}
									else
									{
										addInfos.Remove(key);
									}
								}
							}
						}
					}

					entity[propertyName] = addInfos.CondenseKeyValuePairsIntoSortedOneString();
				}
			}
		}
	}
}
