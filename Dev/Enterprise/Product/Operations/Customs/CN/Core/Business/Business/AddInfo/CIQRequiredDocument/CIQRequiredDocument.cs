using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.CN.Business
{
	public class CIQRequiredDocument : AutoCIQRequiredDocument
	{
		public CIQRequiredDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public CusEntryInstruction CusEntryInstruction => Parent as CusEntryInstruction;

		#region Overrides

		public override bool SupportsNotes => false;

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|XC_DocumentType", Caption = "Document Type", ShortCaption = "Type")]
		[List(nameof(AddInfoLookups) + "." + nameof(CIQRequiredDocumentAddInfoLookups.CIQRequiredDocumentTypes))]
		public override ZString XC_DocumentType
		{
			get => base.XC_DocumentType;
			set
			{
				var oldValue = XC_DocumentType;
				base.XC_DocumentType = value;
				if (!IsCopying && oldValue != XC_DocumentType)
				{
					if (RequestOfNotIssuing)
					{
						XC_NumberOfOriginals = ZInt.Zero;
						XC_NumberOfCopies = ZInt.Zero;
					}
				}
			}
		}

		public bool RequestOfNotIssuing => XC_DocumentType == CIQRequiredDocumentTypeList.Codes._24;

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|XC_NumberOfOriginals", Caption = "Number of Originals", ShortCaption = "Originals")]
		[ReadOnlyMember(nameof(RequestOfNotIssuing))]
		public override ZInt XC_NumberOfOriginals
		{
			get => base.XC_NumberOfOriginals;
			set => base.XC_NumberOfOriginals = value;
		}

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|XC_NumberOfCopies", Caption = "Number of Copies", ShortCaption = "Copies")]
		[ReadOnlyMember(nameof(RequestOfNotIssuing))]
		public override ZInt XC_NumberOfCopies
		{
			get => base.XC_NumberOfCopies;
			set => base.XC_NumberOfCopies = value;
		}

		#endregion

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryInstruction|XC_DocumentName", Caption = "Document Name")]
		public ZString XC_DocumentName => Factory.GetCachedValue<CIQRequiredDocumentTypeList>().GetDescriptionFromCode(XC_DocumentType);

		public ZPropertyInfo XC_DocumentNameInfo => GetZPropertyInfo(nameof(XC_DocumentName));
	}
}
