using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using AutoCusContainer = Enterprise.Customs.Business.AutoCusContainer;

namespace Enterprise.Customs.EU.EMCS.Business
{
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.EMCSCusContainer)]
	public class EMCSCusContainer : AutoEMCSCusContainer
		, Integration.Customs.EUEMCS.ICusContainer
	{
		public EMCSCusContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new static readonly CusContainerTypeDecider TypeDecider = new CusContainerTypeDecider();

		#region Schema

		public new class Schema : AutoCusContainer.Schema
		{
			public const string SealDetails = "SealDetails";
			public const string Comment = "Comment";
			public const int SealDetails_MaxLength = 350;
			public const int Comment_MaxLength = 350;
		}

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore => Res.GetString("9BFF888F-CB60-4A4A-9CA3-4AF15A833040", "Transport");

		protected override Customs.Business.CusContainerValidation GetNewValidation() => new EMCSCusContainerValidation(this);

		public new EMCSCusContainerValidation Validation => (EMCSCusContainerValidation)GetNewValidation();

		#region CO_ContainerNumber

		[ResourceStringData("{EF5F7DA3-BC77-45A1-89B3-4E926C50B472}", Caption = "Identity", ShortCaption = "Id.")]
		public override ZString CO_ContainerNumber
		{
			get => base.CO_ContainerNumber;
			set => base.CO_ContainerNumber = value;
		}

		protected override ZBool IsContainerNumberToUpper => ZBool.False;

		#endregion

		#region CO_Seal

		[ResourceStringData("{6443ACD7-0DFD-4250-B2E8-B1179059BAAE}", Caption = "Seal Number", ShortCaption = "Seal No.")]
		public override ZString CO_Seal
		{
			get => base.CO_Seal;
			set => base.CO_Seal = value;
		}

		protected override ZBool IsSealToUpper => ZBool.False;

		#endregion

		#region ZG_UnitCode

		[ResourceStringData("{F78612EF-2315-43E0-BDA7-4FED9F8BC5E6}", Caption = "Unit Code", ShortCaption = "Code")]
		public override ZString ZG_UnitCode
		{
			get => base.ZG_UnitCode;
			set => base.ZG_UnitCode = value;
		}

		[ResourceStringData("0C256D8B-7E51-47BF-8A99-CA42AEDAA2A2", Caption = "Description")]
		public ZString ZG_UnitCodeDescription
		{
			get
			{
				var unitCode = ZG_UnitCode;
				return Factory.GetCachedValue("5DF45179-4279-4802-AD28-FE3398FD877F|ZG_UnitCodeDescription|" + unitCode,
					() => AddInfoLookups.EMCSDestinationTypeList.GetDescriptionFromCode(unitCode)
				);
			}
		}

		#endregion

		#endregion

		#region Comment

		[MaxLength(Schema.Comment_MaxLength)]
		[BusinessObjectTestExclude]
		[ResourceStringData("{EF09FAAF-D13F-4068-9D6A-3B2448AC1FDB}", Caption = "Complementary Information (Transport Details)")]
		public ZString Comment
		{
			get => ContainerCommentNoteWriter.Value;
			set
			{
				var oldValue = Comment;
				CheckMaximumLength(CommentInfo, value);
				if (ContainerCommentNoteWriter.UpdateValue(value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateComment();
					}
				}
				CommentInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CommentInfo => GetZPropertyInfo(Schema.Comment);

		Customs.Business.PredefinedNoteWriter ContainerCommentNoteWriter
		{
			get { return containerCommentNoteWriter ?? (containerCommentNoteWriter = new Customs.Business.PredefinedNoteWriter(this, PredefinedNoteTypes.Instance.ContainerComment)); }
		}
		Customs.Business.PredefinedNoteWriter containerCommentNoteWriter;

		#endregion

		#region SealDetails

		[MaxLength(Schema.SealDetails_MaxLength)]
		[BusinessObjectTestExclude]
		[ResourceStringData("{49AE79D6-18C4-423E-94A4-4484F3F025AA}", Caption = "Seal Information", ShortCaption = "Seal Information")]
		public ZString SealDetails
		{
			get { return ContainerSealDetailsNoteWriter.Value; }
			set
			{
				var oldValue = SealDetails;
				CheckMaximumLength(SealDetailsInfo, value);
				if (ContainerSealDetailsNoteWriter.UpdateValue(value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateSealDetails();
					}
				}
				SealDetailsInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo SealDetailsInfo => GetZPropertyInfo(Schema.SealDetails);

		Customs.Business.PredefinedNoteWriter ContainerSealDetailsNoteWriter
		{
			get { return containerSealDetailsNoteWriter ?? (containerSealDetailsNoteWriter = new Customs.Business.PredefinedNoteWriter(this, PredefinedNoteTypes.Instance.ContainerSealDetails)); }
		}
		Customs.Business.PredefinedNoteWriter containerSealDetailsNoteWriter;

		#endregion

		public new EMCSJobDeclaration Declaration => (EMCSJobDeclaration)base.Declaration;

		protected override void RegisterJobContainer(ForwardingContainer forwardingContainer)
		{
		}

		protected override void UnRegisterJobContainer(ForwardingContainer forwardingContainer)
		{
		}
	}
}
