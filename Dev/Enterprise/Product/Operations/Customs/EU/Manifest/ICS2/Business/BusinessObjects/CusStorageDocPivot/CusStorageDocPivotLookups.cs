using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class CusStorageDocPivotLookups : Customs.Business.CusStorageDocPivotLookups
	{
		public CusStorageDocPivotLookups(AutoCusStorageDocPivot parent) : base(parent)
		{
		}

		new CusStorageDocPivot Parent => (CusStorageDocPivot)base.Parent;

		public AvailableEDocList AvailableEDocs
		{
			get
			{
				var extensionFilter = Parent.Parent is RequestHeader
					? new List<ZString> { Core.Constants.FileFormats.PDF, Core.Constants.FileFormats.JPG, Core.Constants.FileFormats.JPEG }
					: new List<ZString>();

				var eDocCollections = (Parent.Parent as ICusStorageDocPivotTypeSupporter)?.EDocCollections.ToArray() ?? Array.Empty<IStorageDocsBaseCollection>();
				return new AvailableEDocList(extensionFilter, eDocCollections);
			}
		}
	}
}
