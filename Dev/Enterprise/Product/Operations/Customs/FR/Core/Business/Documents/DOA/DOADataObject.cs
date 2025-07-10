using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Customs.FR.Business.Documents
{
	public class DOADataObject : FRDocDataObject
	{
		#region PortSystem

		public ZString PortSystem
		{
			get => portSystem;
			set
			{
				if (SetNonPersistentPropertyValue(PortSystemInfo, ref portSystem, value))
				{
					Validate(PortSystemInfo);
				}
			}
		}

		ZString portSystem;

		public ZPropertyInfo PortSystemInfo => GetZPropertyInfo(nameof(PortSystem));

		#endregion

		#region SICCodeType

		public ZString SICCodeType
		{
			get => sicCodeType;
			set
			{
				if (SetNonPersistentPropertyValue(SICCodeTypeInfo, ref sicCodeType, value))
				{
					Validate(SICCodeTypeInfo);
				}
			}
		}

		ZString sicCodeType;

		public ZPropertyInfo SICCodeTypeInfo => GetZPropertyInfo(nameof(SICCodeType));

		#endregion

		#region Import / Export

		public ZBool IsImport
		{
			get => isImport;
			set
			{
				if (SetNonPersistentPropertyValue(IsImportInfo, ref isImport, value))
				{
					Validate(IsImportInfo);
				}
			}
		}
		ZBool isImport;

		public ZPropertyInfo IsImportInfo => GetZPropertyInfo(nameof(IsImport));

		#endregion

		#region CurrentUser

		public IAddress CurrentUser
		{
			get => currentUser;
			set => currentUser = SetChild(currentUser, value);
		}

		IAddress currentUser;

		#endregion

		#region SendingPartyID

		public ZString SendingPartyID
		{
			get => sendingPartyID;
			set
			{
				if (SetNonPersistentPropertyValue(SendingPartyIDInfo, ref sendingPartyID, value))
				{
					Validate(SendingPartyIDInfo);
				}
			}
		}

		ZString sendingPartyID;

		public ZPropertyInfo SendingPartyIDInfo => GetZPropertyInfo(nameof(SendingPartyID));

		#endregion

		#region SendingPartySICCode

		public ZString SendingPartySICCode
		{
			get => sendingPartySICCode;
			set
			{
				if (SetNonPersistentPropertyValue(SendingPartySICCodeInfo, ref sendingPartySICCode, value))
				{
					Validate(SendingPartySICCodeInfo);
				}
			}
		}

		ZString sendingPartySICCode;

		public ZPropertyInfo SendingPartySICCodeInfo => GetZPropertyInfo(nameof(SendingPartySICCode));

		#endregion

		#region RecipientID

		public ZString RecipientID
		{
			get => recipientID;
			set
			{
				if (SetNonPersistentPropertyValue(RecipientIDInfo, ref recipientID, value))
				{
					Validate(RecipientIDInfo);
				}
			}
		}

		ZString recipientID;

		public ZPropertyInfo RecipientIDInfo => GetZPropertyInfo(nameof(RecipientID));

		#endregion

		#region RecipientSICCode

		public ZString RecipientSICCode
		{
			get => recipientSICCode;
			set
			{
				if (SetNonPersistentPropertyValue(RecipientSICCodeInfo, ref recipientSICCode, value))
				{
					Validate(RecipientSICCodeInfo);
				}
			}
		}

		ZString recipientSICCode;

		public ZPropertyInfo RecipientSICCodeInfo => GetZPropertyInfo(nameof(RecipientSICCode));

		#endregion

		#region CTOPartyID

		public ZString CTOPartyID
		{
			get => ctoPartyID;
			set
			{
				if (SetNonPersistentPropertyValue(CTOPartyIDInfo, ref ctoPartyID, value))
				{
					Validate(CTOPartyIDInfo);
				}
			}
		}

		ZString ctoPartyID;

		public ZPropertyInfo CTOPartyIDInfo => GetZPropertyInfo(nameof(CTOPartyID));

		#endregion

		#region CTOPartySICCode

		public ZString CTOPartySICCode
		{
			get => ctoPartySICCode;
			set
			{
				if (SetNonPersistentPropertyValue(CTOPartySICCodeInfo, ref ctoPartySICCode, value))
				{
					Validate(CTOPartySICCodeInfo);
				}
			}
		}

		ZString ctoPartySICCode;

		public ZPropertyInfo CTOPartySICCodeInfo => GetZPropertyInfo(nameof(CTOPartySICCode));

		#endregion

		#region JobNumber

		public ZString JobNumber
		{
			get => jobNumber;
			set
			{
				if (SetNonPersistentPropertyValue(JobNumberInfo, ref jobNumber, value))
				{
					Validate(JobNumberInfo);
				}
			}
		}
		ZString jobNumber;

		public ZPropertyInfo JobNumberInfo => GetZPropertyInfo(nameof(JobNumber));

		#endregion

		#region TotalNumberOfPacks

		public ZInt TotalNumberOfPacks
		{
			get => totalNumberOfPacks;
			set
			{
				if (SetNonPersistentPropertyValue(TotalNumberOfPacksInfo, ref totalNumberOfPacks, value))
				{
					Validate(TotalNumberOfPacksInfo);
				}
			}
		}
		ZInt totalNumberOfPacks;

		public ZPropertyInfo TotalNumberOfPacksInfo => GetZPropertyInfo(nameof(TotalNumberOfPacks));

		#endregion

		#region PackageType

		public ICodeDescription PackageType
		{
			get => packageType;
			set => packageType = SetChild(packageType, value);
		}
		ICodeDescription packageType;

		#endregion

		#region TotalGrossWeightInKilograms

		public ZDecimal TotalGrossWeightInKilograms
		{
			get => totalGrossWeightInKilograms;
			set
			{
				if (SetNonPersistentPropertyValue(TotalGrossWeightInKilogramsInfo, ref totalGrossWeightInKilograms, value))
				{
					Validate(TotalGrossWeightInKilogramsInfo);
				}
			}
		}
		ZDecimal totalGrossWeightInKilograms;

		public ZPropertyInfo TotalGrossWeightInKilogramsInfo => GetZPropertyInfo(nameof(TotalGrossWeightInKilograms));

		#endregion

		#region TotalNetWeightInKilograms

		public ZDecimal TotalNetWeightInKilograms
		{
			get => totalNetWeightInKilograms;
			set
			{
				if (SetNonPersistentPropertyValue(TotalNetWeightInKilogramsInfo, ref totalNetWeightInKilograms, value))
				{
					Validate(TotalNetWeightInKilogramsInfo);
				}
			}
		}
		ZDecimal totalNetWeightInKilograms;

		public ZPropertyInfo TotalNetWeightInKilogramsInfo => GetZPropertyInfo(nameof(TotalNetWeightInKilograms));

		#endregion

		#region CommonAccessRef

		public ZString CommonAccessRef
		{
			get => commonAccessRef;
			set
			{
				if (SetNonPersistentPropertyValue(CommonAccessRefInfo, ref commonAccessRef, value))
				{
					Validate(CommonAccessRefInfo);
				}
			}
		}
		ZString commonAccessRef;

		public ZPropertyInfo CommonAccessRefInfo => GetZPropertyInfo(nameof(CommonAccessRef));

		#endregion

		#region AppliesToAllPacks

		public ZBool AppliesToAllPacks
		{
			get => appliesToAllPacks;
			set
			{
				if (SetNonPersistentPropertyValue(AppliesToAllPacksInfo, ref appliesToAllPacks, value))
				{
					Validate(AppliesToAllPacksInfo);
				}
			}
		}
		ZBool appliesToAllPacks;

		public ZPropertyInfo AppliesToAllPacksInfo => GetZPropertyInfo(nameof(AppliesToAllPacks));

		#endregion

		#region DeclarationType

		public ZString DeclarationType
		{
			get => declarationType;
			set
			{
				if (SetNonPersistentPropertyValue(DeclarationTypeInfo, ref declarationType, value))
				{
					Validate(DeclarationTypeInfo);
				}
			}
		}
		ZString declarationType;

		public ZPropertyInfo DeclarationTypeInfo => GetZPropertyInfo(nameof(DeclarationType));

		#endregion

		#region DeclarationReference

		public ZString DeclarationReference
		{
			get => declarationReference;
			set
			{
				if (SetNonPersistentPropertyValue(DeclarationReferenceInfo, ref declarationReference, value))
				{
					Validate(DeclarationReferenceInfo);
				}
			}
		}
		ZString declarationReference;

		public ZPropertyInfo DeclarationReferenceInfo => GetZPropertyInfo(nameof(DeclarationReference));

		#endregion

		#region DeclarationStatus

		public ZString DeclarationStatus
		{
			get => declarationStatus;
			set
			{
				if (SetNonPersistentPropertyValue(DeclarationStatusInfo, ref declarationStatus, value))
				{
					Validate(DeclarationStatusInfo);
				}
			}
		}
		ZString declarationStatus;

		public ZPropertyInfo DeclarationStatusInfo => GetZPropertyInfo(nameof(DeclarationStatus));

		#endregion

		#region IsPrelodged

		public ZBool IsPrelodged
		{
			get => isPrelodged;
			set
			{
				if (SetNonPersistentPropertyValue(IsPrelodgedInfo, ref isPrelodged, value))
				{
					Validate(IsPrelodgedInfo);
				}
			}
		}
		ZBool isPrelodged;

		public ZPropertyInfo IsPrelodgedInfo => GetZPropertyInfo(nameof(IsPrelodged));

		#endregion

		#region DeclarantsSIRETNumber

		public ZString DeclarantsSIRETNumber
		{
			get => declarantsSIRETNumber;
			set
			{
				if (SetNonPersistentPropertyValue(DeclarantsSIRETNumberInfo, ref declarantsSIRETNumber, value))
				{
					Validate(DeclarantsSIRETNumberInfo);
				}
			}
		}

		ZString declarantsSIRETNumber;

		public ZPropertyInfo DeclarantsSIRETNumberInfo => GetZPropertyInfo(nameof(DeclarantsSIRETNumber));

		#endregion

		#region CustomsOfficeCodeOfDeparture

		public ICodeDescription CustomsOfficeCodeOfDeparture
		{
			get => customsOfficeCodeOfDeparture;
			set => customsOfficeCodeOfDeparture = SetChild(customsOfficeCodeOfDeparture, value);
		}

		ICodeDescription customsOfficeCodeOfDeparture;

		#endregion

		#region CustomsOfficeCodeOfDestination

		public ICodeDescription CustomsOfficeCodeOfDestination
		{
			get => customsOfficeCodeOfDestination;
			set => customsOfficeCodeOfDestination = SetChild(customsOfficeCodeOfDestination, value);
		}

		ICodeDescription customsOfficeCodeOfDestination;

		#endregion

		#region Port

		public ZString Port
		{
			get => port;
			set
			{
				if (SetNonPersistentPropertyValue(PortInfo, ref port, value))
				{
					Validate(PortInfo);
				}
			}
		}

		ZString port;
		public ZPropertyInfo PortInfo => GetZPropertyInfo(nameof(Port));
		#endregion

		#region PortDuesAmount

		public ZDecimal PortDuesAmount
		{
			get => portDuesInfo;
			set
			{
				if (SetNonPersistentPropertyValue(PortDuesAmountInfo, ref portDuesInfo, value))
				{
					Validate(PortDuesAmountInfo);
				}
			}
		}
		ZDecimal portDuesInfo;

		public ZPropertyInfo PortDuesAmountInfo => GetZPropertyInfo(nameof(PortDuesAmount));

		#endregion

		#region PortDuesCurrency

		public ICodeDescription PortDuesCurrency
		{
			get => portDuesCurrency;
			set => portDuesCurrency = SetChild(portDuesCurrency, value);
		}

		ICodeDescription portDuesCurrency;

		#endregion

		#region HasSeal

		public ZBool HasSeal
		{
			get => hasSeal;
			set
			{
				if (SetNonPersistentPropertyValue(HasSealInfo, ref hasSeal, value))
				{
					Validate(HasSealInfo);
				}
			}
		}
		ZBool hasSeal;

		public ZPropertyInfo HasSealInfo => GetZPropertyInfo(nameof(HasSeal));

		#endregion

		#region Tariffs

		public List<ICodeDescription> Tariffs
		{
			get => tariffs;
			set => tariffs = SetChild(tariffs, value);
		}
		List<ICodeDescription> tariffs;

		#endregion

		#region DeclarationNumber

		public ZString DeclarationNumber
		{
			get => declarationNumber;
			set
			{
				if (SetNonPersistentPropertyValue(DeclarationNumberInfo, ref declarationNumber, value))
				{
					Validate(DeclarationNumberInfo);
				}
			}
		}
		ZString declarationNumber;

		public ZPropertyInfo DeclarationNumberInfo => GetZPropertyInfo(nameof(DeclarationNumber));

		#endregion DeclarationNumber

		#region EquipmentRef

		public ZString EquipmentRef
		{
			get => equipmentRef;
			set
			{
				if (SetNonPersistentPropertyValue(EquipmentRefInfo, ref equipmentRef, value))
				{
					Validate(EquipmentRefInfo);
					Validate(CommonAccessRefInfo);
				}
			}
		}
		ZString equipmentRef;

		public ZPropertyInfo EquipmentRefInfo => GetZPropertyInfo(nameof(EquipmentRef));

		#endregion EquipmentRef

		#region Containers

		public IReadOnlyCollection<Container> Containers
		{
			get => containers;
			set => containers = SetChildCollection(containers, value);
		}

		IReadOnlyCollection<Container> containers;

		#endregion Containers

		#region ContainerMessage

		public ZString ContainerMessage
		{
			get => containerMessage;
			set
			{
				if (SetNonPersistentPropertyValue(ContainerMessageInfo, ref containerMessage, value))
				{
					Validate(ContainerMessageInfo);
				}
			}
		}

		ZString containerMessage;

		public ZPropertyInfo ContainerMessageInfo => GetZPropertyInfo(nameof(ContainerMessage));

		#endregion ContainerMessage
	}
}
