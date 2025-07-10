//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoCusStorageDocPivotLookups
//
//    This class should be used for overriding collections in AutoCusStorageDocPivotLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusStorageDocPivotLookups : Customs.Business.CusStorageDocPivotLookups
	{
		public CusStorageDocPivotLookups(AutoCusStorageDocPivot parent) : base(parent)
		{
		}

		new CusStorageDocPivot Parent => (CusStorageDocPivot)base.Parent;

		public CodeDescriptionPairList AttachmentTypeList
		{
			get
			{
				CodeDescriptionPairList result = null;
				if (Parent.Parent is QuarantineColsHeader)
				{
					result = RefCusCodeListTypes.GetCachedList(Factory
						, Core.Constants.CountryCodes.Australia
						, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AUCOLSAttachmentType
						, ZDateTime.Today);
				}
				else
				{
					result = RefCusCodeListTypes.GetCachedList(Factory
							   , Core.Constants.CountryCodes.Australia
							   , Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NEXDOCSAttachmentType
							   , ZDateTime.Today);
				}
				return result;
			}
		}

		public CodeDescriptionPairList COLSDocumentStatusList
		{
			get
			{
				CodeDescriptionPairList result = null;
				if (Parent.Parent is QuarantineColsHeader)
				{
					result = Factory.GetCachedValue<COLSDocumentStatusList>();
				}
				return result;
			}
		}

		public AvailableEDocList AvailableEDocs
		{
			get
			{
				var extensionFilter = new List<ZString>();
				var eDocCollections = (Parent.Parent as ICusStorageDocPivotParent)?.EDocCollections.ToArray() ?? Array.Empty<IStorageDocsBaseCollection>();
				return new AvailableEDocList(extensionFilter, eDocCollections);
			}
		}
	}
}
