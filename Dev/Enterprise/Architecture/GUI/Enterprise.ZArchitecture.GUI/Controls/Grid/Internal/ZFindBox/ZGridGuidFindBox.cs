using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public class ZGridGuidFindBox : ZGridFindBox
	{
		public ZGridGuidFindBox()
		{
		}

		GuidCache? guidCache;

		public bool IsGuidCacheEnabled { get; set; } = true;

		public ZGuid Guid
		{
			get
			{
				var currentCode = Code;
				var currentList = ListProvider.List;
				if (string.IsNullOrEmpty(currentCode))
				{
					return ZGuid.Empty;
				}
				else if (!guidCache.HasValue || !guidCache.Value.Code.Equals(currentCode, StringComparison.OrdinalIgnoreCase) || guidCache.Value.List != currentList || !IsGuidCacheEnabled)
				{
					var guid = ListProvider.PrimaryKeyFromCode(currentCode);
					if (IsGuidCacheEnabled)
					{
						guidCache = new GuidCache(guid, currentCode, currentList);
					}

					return guid;
				}
				else
				{
					return guidCache.Value.Guid;
				}
			}
		}

		protected override void OnPopupSelected(IFindBoxPopup popup, BusinessObject[] selectedBusinessObjects)
		{
			base.OnPopupSelected(popup, selectedBusinessObjects);

			if (selectedBusinessObjects.Length == 1)
			{
				var bizObj = selectedBusinessObjects[0];
				if (IsGuidCacheEnabled)
				{
					guidCache = new GuidCache(bizObj.PK, ((ICodeDescription)bizObj).Code, ListProvider.List);
				}
			}
		}

		protected
#if DEBUG
		internal
#endif
		override IEnumerable<BusinessObject> GetBizObjsToEditOrView()
		{
			var bizObjs = base.GetBizObjsToEditOrView();

			var guid = (CurrentItem as BusinessObject)?.FindPropertyInfo(DataPropertyName)?.Value as ZGuid? ?? Guid;
			if (guid.IsValid)
			{
				var bizObjWithMatchingPk = bizObjs.FirstOrDefault(x => x.PK == guid);
				if (bizObjWithMatchingPk != null)
				{
					return new BusinessObject[] { bizObjWithMatchingPk };
				}
			}

			return bizObjs;
		}

		struct GuidCache
		{
			public GuidCache(ZGuid guid, string code, IBusinessObjectCollection list)
			{
				Guid = guid;
				Code = code;
				List = list;
			}

			public readonly ZGuid Guid;
			public readonly string Code;
			public readonly IBusinessObjectCollection List;
		}
	}
}
