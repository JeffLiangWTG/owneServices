using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Business
{
	public class MSXMessageSendingObjectAttachmentLookups : ZLookups
	{
		public MSXMessageSendingObjectAttachmentLookups(MSXMessageSendingObjectAttachment parent) : base(parent)
		{
		}

		public new MSXMessageSendingObjectAttachment Parent => (MSXMessageSendingObjectAttachment)base.Parent;

		protected override BusinessObjectFactory Factory => Parent.Parent.Factory;

		public ICodeDescriptionPairList FileList => new AvailableEDocList(new List<ZString>() { }, GetEDocCollections());

		public IStorageDocsBaseCollection[] GetEDocCollections()
		{
			return Parent.Parent?.EDocCollections.ToArray() ?? Array.Empty<IStorageDocsBaseCollection>();
		}

		public ICodeDescriptionPairList TypeList
		{
			get
			{
				var codeDescriptionList = new CodeDescriptionPairList();
				var today = ZDateTime.Today;
				var codes = Factory.GetCachedValue($"JP.MSXMessageSendingObjectAttachmentsLookups.TypeList.{today}", () =>
				{
					return ZZRefCusCodeListCombined.Loader.Load(Factory, Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanDocumentTypes, today);
				});
				codeDescriptionList.AddRange(codes);
				return codeDescriptionList;
			}
		}
	}
}
