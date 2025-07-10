using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.IL.Business
{
	public partial class JobDeclaration : AutoILJobDeclaration, IAdditionalBusinessObjectFetchStrategyProvider, ICusSupportingInfoTypeSupporter, IEDIMessageCollectionOwner
	{
		public new class Schema : BaseJobDeclaration.Schema
		{
			public const string JE_MasterBillIssuedDate = "JE_MasterBillIssuedDate";
			public const int JE_ManifestNumberMaxLengthSea = 6;
			public const int JE_ManifestNumberMaxLengthRoa = 15;
			public new const int JE_LocationOfGoodsMaxLength = 9;
		}

		public JobDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("Enterprise.Customs.IL.Business.JE_MessageType", Caption = "Entry Type")]
		public override ZString JE_MessageType
		{
			get => base.JE_MessageType;
			set
			{
				if (IsJE_MessageTypeSettingSuspended || SetterSuspender.IsSetterSuspended("JE_MessageType"))
				{
					return;
				}

				base.JE_MessageType = value;

				if (!IsCopying)
				{
					SetMsgSubtypeBasedOnMsgType();
					SetTransportMeansBaseOnMsgTypeAndTransportMode();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.IL.Business.JE_MessageSubType", Caption = "Entry Style")]
		public override ZString JE_MessageSubType { get => base.JE_MessageSubType; set => base.JE_MessageSubType = value; }

		[ResourceStringData("Enterprise.Customs.IL.Business.DeclarationNumber", Caption = "Declaration Number")]
		public override ZString DeclarationNumber { get => base.DeclarationNumber; set => base.DeclarationNumber = value; }

		[ResourceStringData("Enterprise.Customs.IL.Business.JE_TransportMeans", Caption = "Cargo Type Code")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.TransportMeansList))]
		public override ZString JE_TransportMeans { get => base.JE_TransportMeans; set => base.JE_TransportMeans = value; }

		public override ZString JE_TransportMode
		{
			get => base.JE_TransportMode;
			set
			{
				base.JE_TransportMode = value;

				if (!IsCopying)
				{
					SetTransportMeansBaseOnMsgTypeAndTransportMode();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.IL.Business.JE_MasterBillIssuedDate", Caption = "Issue Date Time")]
		public override ZDateTime JE_MasterBillIssuedDate { get => base.JE_MasterBillIssuedDate; set => base.JE_MasterBillIssuedDate = value; }

		[ResourceStringData("Enterprise.Customs.IL.Business.JE_RL_NKOrigin", Caption = "Port Of Origin")]
		public override ZString JE_RL_NKOrigin { get => base.JE_RL_NKOrigin; set => base.JE_RL_NKOrigin = value; }

		[ResourceStringData("Enterprise.Customs.IL.Business.JE_RL_NKFinalDestination", Caption = "Final Destination")]
		public override ZString JE_RL_NKFinalDestination { get => base.JE_RL_NKFinalDestination; set => base.JE_RL_NKFinalDestination = value; }

		[ResourceStringData("Enterprise.Customs.IL.Business.JE_LocationOfGoods", Caption = "Goods Location")]
		[List(nameof(Lookups) + "." + nameof(JobDeclarationLookups.LocationOfGoodsCollection))]
		[MaxLength(Schema.JE_LocationOfGoodsMaxLength)]
		public override ZString JE_LocationOfGoods { get => base.JE_LocationOfGoods; set => base.JE_LocationOfGoods = value; }

		[ResourceStringData("Enterprise.Customs.IL.Business.JE_TotalNoOfPacks", Caption = "Quantity")]
		public override ZInt JE_TotalNoOfPacks { get => base.JE_TotalNoOfPacks; set => base.JE_TotalNoOfPacks = value; }

		[ResourceStringData("Enterprise.Customs.IL.Business.JE_CustomsOffice", Caption = "Customs Office")]
		public override ZString JE_CustomsOffice { get => base.JE_CustomsOffice; set => base.JE_CustomsOffice = value; }

		[ResourceStringData("Enterprise.Customs.IL.Business.JE_ManifestNumber", Caption = "Manifest")]
		public override ZString JE_ManifestNumber { get => base.JE_ManifestNumber; set => base.JE_ManifestNumber = value; }

		[ResourceStringData("Enterprise.Customs.IL.Business.JE_OA_DeclarantAddress", Caption = "Declarant")]
		public override ZGuid JE_OA_DeclarantAddress { get => base.JE_OA_DeclarantAddress; set => base.JE_OA_DeclarantAddress = value; }

		[ResourceStringData("Enterprise.Customs.IL.Business.JE_OA_Representative", Caption = "Representative")]
		public override ZGuid JE_OA_Representative { get => base.JE_OA_Representative; set => base.JE_OA_Representative = value; }

		[ResourceStringData("Enterprise.Customs.IL.Business.JE_OA_SellerAddress", Caption = "Seller")]
		public override ZGuid JE_OA_SellerAddress { get => base.JE_OA_SellerAddress; set => base.JE_OA_SellerAddress = value; }

		public override ZBool AreMultipleEntryInstructionsAllowed => false;

		public override bool ContainerModeVisible => true;

		[ChildEditable(true)]
		public SupportingDocumentCollection SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					supportingDocuments = new SupportingDocumentCollection(this);
					supportingDocuments.Load();
					RegisterEditableChildObject(supportingDocuments);
				}
				return supportingDocuments;
			}
		}
		SupportingDocumentCollection supportingDocuments;

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		[UniversalCopyCollectionEntity(CusEntryInstructionSchema.Constants.TableName, CusEntryInstructionSchema.Constants.CEI_JE)]
		public new ICusEntryInstructionCollection<CusEntryInstruction> CustomsEntryInstructions => (ICusEntryInstructionCollection<CusEntryInstruction>)base.CustomsEntryInstructions;

		public new EntryInstructionProvider CustomsEntryInstructionProvider => (EntryInstructionProvider)base.CustomsEntryInstructionProvider;
		protected override Customs.Business.EntryInstructionProvider GetCustomsEntryInstructionProviderCore() => new EntryInstructionProvider(this);

		#region ICusSupportingInfoTypeSupporter

		IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			return new Dictionary<ZString, Type> {
				{ Common.IL.CusSupportingInfoTypeList.Codes.SupportingDocument, typeof(SupportingDocument) },
			};
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}

		#endregion

		protected override bool IsCustomsHeaderAmendmentATotalReplacement => false;

		protected override bool IsCustomsLineAmendmentATotalReplacement => false;

		protected override ZString LocalCurrencyCodeCore => Core.Constants.CurrencyCodes.Israel;

		protected override bool HasSplitEntriesCore
		{
			get
			{
				if (!GetType().FullName.Contains("IL"))
				{
					ErrorReporter.ReportOnce("This method must be implemented before messaging is written", "This method must be implemented before messaging is written");
				}
				return base.HasSplitEntriesCore;
			}
		}

		[ResourceStringData("9890F79B-E1AA-445D-B910-CCF023635DD5", Caption = "Flight", IsApplicableMember = nameof(IsAir))]
		[ResourceStringData("6D1CC23D-34E6-4E0F-B7EF-8A4BE611DFDC", Caption = "Voyage", IsApplicableMember = nameof(IsRoad))]
		public override ZString JE_VoyageFlightNo { get => base.JE_VoyageFlightNo; set => base.JE_VoyageFlightNo = value; }

		protected override JobDeclarationSynchroniser GetNewShipmentSynchroniser() => new ILJobDeclarationSynchroniser(this);

		protected override Customs.Business.MergeManager GetMergeManager() => new MergeManager(this);

		void SetMsgSubtypeBasedOnMsgType()
		{
			switch (JE_MessageType)
			{
				case ILDeclarationMessageTypeList.Codes.Import:
					JE_MessageSubType = ILDeclarationMessageSubTypeList.Codes.Import;
					break;
				case ILDeclarationMessageTypeList.Codes.Export:
					JE_MessageSubType = ILDeclarationMessageSubTypeList.Codes.Export;
					break;
				default:
					break;
			}
		}

		void SetTransportMeansBaseOnMsgTypeAndTransportMode()
		{
			switch (JE_MessageType)
			{
				case ILDeclarationMessageTypeList.Codes.Import:
					JE_TransportMeans = (string)JE_TransportMode switch
					{
						Core.Constants.TransportModes.Air => Constants.CargoIdentifierType.AirBillOfLadingImport,
						Core.Constants.TransportModes.Sea => Constants.CargoIdentifierType.SeaDealImport,
						Core.Constants.TransportModes.Road => Constants.CargoIdentifierType.LandDealImport,
						_ => ZString.Empty
					};
					break;
				case ILDeclarationMessageTypeList.Codes.Export:
					JE_TransportMeans = (string)JE_TransportMode switch
					{
						Core.Constants.TransportModes.Air => Constants.CargoIdentifierType.AirBillOfLadingExport,
						Core.Constants.TransportModes.Sea => Constants.CargoIdentifierType.PortExportDeliveryDocument,
						Core.Constants.TransportModes.Road => Constants.CargoIdentifierType.LandExportDeliveryDocument,
						_ => ZString.Empty
					};
					break;
				default:
					break;
			}
		}

		#region IEDIMessageCollectionOwner

		BusinessObject IEDIMessageCollectionOwner.MessageOwner => this;

		IBusinessObjectCollection IEDIMessageCollectionOwner.Messages => Messages;

		#endregion IEDIMessageCollectionOwner
	}
}
