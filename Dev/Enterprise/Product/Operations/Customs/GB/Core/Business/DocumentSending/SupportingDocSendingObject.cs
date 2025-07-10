using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.MessageBuilders.DocumentSending;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.DocumentSending
{
	[TestExcludeBusinessObjectsAllHaveTestCases]
	public partial class SupportingDocSendingObject : Customs.Business.JobDeclarationSupportingDocSendingObject
	{
		public SupportingDocSendingObject(JobDeclaration declaration) : base(declaration)
		{
		}

		public static SupportingDocSendingObject New(JobDeclaration declaration)
		{
			var result = new SupportingDocSendingObject(declaration)
			{
				ShouldSend = true
			};
			result.DefaultLocalReferenceNumber();
			return result;
		}

		protected override ZString DocumentTypeCode => Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocument; // Needs to be changed when we know the code

		public override bool ShouldCheckFileNameInEdocField => true;

		public override SupportingDocUniversalEventBuilder GetSupportingDocUniversalEventBuilder()
		{
			return new GBSupportingDocUniversalEventBuilder(this);
		}

		protected override Customs.Business.SupportingDocSendingObjectValidation GetNewValidation()
		{
			return new SupportingDocSendingObjectValidation(this);
		}

		public override CodeDescriptionPairList Entries
		{
			get
			{
				var codeDescriptionPairList = new CodeDescriptionPairList();
				var validEntryHeaders = Declaraction?.ActiveEntryHeaders.OfType<CusEntryHeader>()?.Where(ah => !ah.MovementReferenceNumber.IsEmpty || !ah.LRN.IsEmpty) ?? Enumerable.Empty<CusEntryHeader>();
				foreach (var validEntryHeader in validEntryHeaders)
				{
					var zString = !validEntryHeader.MovementReferenceNumber.IsEmpty ? validEntryHeader.MovementReferenceNumber : validEntryHeader.LRN;
					var text = !validEntryHeader.MovementReferenceNumber.IsEmpty ? CusEntryNumberTypes.Standard.MovementReferenceNumber : CusEntryNumberTypes.Standard.LocalReferenceNumber;
					codeDescriptionPairList.Add(new CodeDescriptionPair(zString.ToString(), Res.GetString("886E83BD-1347-4748-B57C-2DB86E89659B", "{0}: {1}", text, zString)));
				}
				return codeDescriptionPairList;
			}
		}

		public new SupportingDocSendingObjectValidation Validation => (SupportingDocSendingObjectValidation)base.Validation;
	}
}
