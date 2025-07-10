using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public class EntryInstructionAttachmentLookups : ZLookups
	{
		public EntryInstructionAttachmentLookups(EntryInstructionAttachment parent) : base(parent)
		{
		}

		CusEntryInstruction EntryInstruction => ((EntryInstructionAttachment)Parent).EntryInstruction;

		public CodeDescriptionPairList AttachmentTypeList
		{
			get
			{
				var declaration = EntryInstruction?.JobDeclaration;
				return declaration == null ? new CodeDescriptionPairList() : CSDDocTypeList.GetCachedCSDDocTypeList(declaration.Factory, declaration.IsImport);
			}
		}

		public ICodeDescriptionPairList EDocList => new AvailableEDocList(new List<ZString>() { Core.Constants.FileFormats.PDF }, GetEDocCollections());

		public IStorageDocsBaseCollection[] GetEDocCollections()
		{
			return EntryInstruction?.EDocCollections.ToArray() ?? Array.Empty<IStorageDocsBaseCollection>();
		}
	}
}
