using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DocManagerIncludingRelatedObjectsInfo : DocManagerInfo
	{
		public DocManagerIncludingRelatedObjectsInfo(IDocManagerSupportIncudingRelatedObjects parent, string docManagerCode)
			: base(parent.SelfReference, docManagerCode)
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var result = new List<BusinessObject>();
			if (BusinessEntity != null)
			{
				result.AddRange(GetRelatedObjects((IDocManagerSupportIncudingRelatedObjects)BusinessEntity));
			}
			return result.ToArray();
		}

		IEnumerable<BusinessObject> GetRelatedObjects(IDocManagerSupportIncudingRelatedObjects parent)
		{
			foreach (var relatedObj in parent.GetRelatedBusinessObjects())
			{
				yield return relatedObj;

				var childAsParent = relatedObj as IDocManagerSupportIncudingRelatedObjects;
				if (childAsParent != null)
				{
					foreach (var relatedObjInChild in GetRelatedObjects(childAsParent))
					{
						yield return relatedObjInChild;
					}
				}
			}
		}
	}
}
