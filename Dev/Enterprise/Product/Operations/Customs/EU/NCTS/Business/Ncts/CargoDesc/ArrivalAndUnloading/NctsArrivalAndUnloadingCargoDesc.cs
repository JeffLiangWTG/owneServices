using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.FetchStrategies;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.EU.NCTS.Business.ResultOfCOntrol;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsArrivalAndUnloadingCargoDesc : NctsCommonCargoDesc
		, Integration.Customs.EU.NCTS.IArrivalAndUnloadingCargoDesc
		, Integration.Customs.ICusAddInfoTypeSupporter
		, ICusInBondContainerTypeSupporter
	{
		public NctsArrivalAndUnloadingCargoDesc(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new NctsArrivalAndUnloadingCargoDescLookups Lookups => (NctsArrivalAndUnloadingCargoDescLookups)base.Lookups;

		public new NctsArrivalAndUnloadingCargoDescValidation Validation => (NctsArrivalAndUnloadingCargoDescValidation)base.Validation;

		[ReadOnlyMember(nameof(UnloadedGoodsItemsReadOnly))]
		public override ZString BY_HarmonisedTariff
		{
			get => base.BY_HarmonisedTariff;
			set => base.BY_HarmonisedTariff = value;
		}

		[ReadOnlyMember(nameof(UnloadedGoodsItemsReadOnly))]
		public override ZString BY_Description
		{
			get => base.BY_Description;
			set => base.BY_Description = value;
		}

		[ReadOnlyMember(nameof(UnloadedGoodsItemsReadOnly))]
		public override ZDecimal BY_GrossWeight
		{
			get => base.BY_GrossWeight;
			set => base.BY_GrossWeight = value;
		}

		[ReadOnlyMember(nameof(UnloadedGoodsItemsReadOnly))]
		public override ZString BY_GrossWeightUnit
		{
			get => base.BY_GrossWeightUnit;
			set => base.BY_GrossWeightUnit = value.ToUpper();
		}

		[ReadOnlyMember(nameof(UnloadedGoodsItemsReadOnly))]
		public override ZDecimal BY_NetWeight
		{
			get => base.BY_NetWeight;
			set => base.BY_NetWeight = value;
		}

		[ReadOnlyMember(nameof(UnloadedGoodsItemsReadOnly))]
		public override ZString BY_NetWeightUnit
		{
			get => base.BY_NetWeightUnit;
			set => base.BY_NetWeightUnit = value.ToUpper();
		}

		public override ZString BY_ParentTableCode
		{
			get => base.BY_ParentTableCode;
			set
			{
				var oldValue = BY_ParentTableCode;
				base.BY_ParentTableCode = value;
				if (!IsCopying && oldValue != BY_ParentTableCode)
				{
					Containers.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid BY_ParentID
		{
			get => base.BY_ParentID;
			set
			{
				var oldValue = BY_ParentID;
				base.BY_ParentID = value;
				if (!IsCopying && oldValue != BY_ParentID)
				{
					Containers.MarkAsNeedingValidation();
				}
			}
		}

		/// <summary>
		/// Only used to store/reteive Unloaded containars for IE43/IE44
		/// For departure containers use ContainersPivots and ContainersSelected
		/// </summary>
		[ChildEditable]
		public NctsCommonCargoDescContainerCollection Containers //Unloaded Containers
		{
			get
			{
				if (containers == null)
				{
					containers = GetContainersCollection();
					RegisterEditableChildObject(containers);
				}
				return containers;
			}
		}

		NctsCommonCargoDescContainerCollection containers;

		protected virtual NctsCommonCargoDescContainerCollection GetContainersCollection() => new NctsCommonCargoDescContainerCollection(this);

		Type ICusInBondContainerTypeSupporter.ContainerType => typeof(NctsContainer);

		// 199 occurrences
		[ChildEditable(true)]
		public CusAddInfoCollection<ResultsOfControlAddInfo> ResultsOfControlCollection => resultsOfControlCollection ?? (resultsOfControlCollection = GetResultsOfControlCollection());
		CusAddInfoCollection<ResultsOfControlAddInfo> resultsOfControlCollection;

		CusAddInfoCollection<ResultsOfControlAddInfo> GetResultsOfControlCollection()
		{
			var result = new CusAddInfoCollection<ResultsOfControlAddInfo>(this);
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		const string ItemCheckedPointer = "OK";

		static ZBool GetCheckItemValue(ZString pointerToTheAttribute, ZBool defaultValue, CusAddInfoCollection<ResultsOfControlAddInfo> collection)
		{
			var resultsOfControl = collection.OfType<CusAddInfo<ResultsOfControlAddInfo>>().FirstOrDefault(r => r.Data.G9_PointerToTheAttribute == pointerToTheAttribute);
			return resultsOfControl == null ? (bool)defaultValue : resultsOfControl.Data.G9_CorrectedValue == "1";
		}

		static void SetCheckItemValue(ZString pointerToTheAttribute, ZBool newValue, CusAddInfoCollection<ResultsOfControlAddInfo> collection)
		{
			var resultsOfControl = collection.OfType<CusAddInfo<ResultsOfControlAddInfo>>().FirstOrDefault(r => r.Data.G9_PointerToTheAttribute == pointerToTheAttribute);
			if (resultsOfControl == null)
			{
				resultsOfControl = collection.AddNew();
				resultsOfControl.Data.G9_PointerToTheAttribute = pointerToTheAttribute;
			}
			resultsOfControl.Data.G9_CorrectedValue = newValue ? "1" : "0";
		}

		void ClearUnloadedItem()
		{
			BY_HarmonisedTariff = ZString.Empty;
			BY_Description = ZString.Empty;
			BY_GrossWeight = ZDecimal.Zero;
			BY_NetWeight = ZDecimal.Zero;
			SupportingDocuments.RemoveAndDeleteAll();
			Containers.DeleteAll();
			Packages.RemoveAndDeleteAll();
			AdditionalInfos.RemoveAndDeleteAll();
			UnloadingNotes = ZString.Empty;
		}

		void RestoreUnloadedItem()
		{
			var header = Header;
			var arrivalMovementHeader = header?.ArrivalMovementHeader;
			if (header != null && arrivalMovementHeader != null && arrivalMovementHeader.GoodsItems.Count > 0)
			{
				var lineNo = BY_LineNo;
				var originalItem = arrivalMovementHeader.GoodsItems.FirstOrDefault(x => x.BY_LineNo == lineNo);
				if (originalItem != null)
				{
					BY_HarmonisedTariff = originalItem.BY_HarmonisedTariff;
					BY_Description = originalItem.BY_Description;
					BY_GrossWeight = originalItem.BY_GrossWeight;
					BY_GrossWeightUnit = originalItem.BY_GrossWeightUnit;
					BY_NetWeight = originalItem.BY_NetWeight;
					BY_NetWeightUnit = originalItem.BY_NetWeightUnit;
					UnloadingNotes = ZString.Empty;

					SupportingDocuments.RemoveAndDeleteAll();
					foreach (NctsSupportingDocument sd in originalItem.SupportingDocuments)
					{
						var newDoc = SupportingDocuments.AddNew();
						newDoc.CSI_Code = sd.CSI_Code;
						newDoc.CSI_ReferenceNumber = sd.CSI_ReferenceNumber;
						newDoc.CSI_Description = sd.CSI_Description;
					}

					Packages.RemoveAndDeleteAll();
					foreach (NctsPackage pack in originalItem.Packages)
					{
						var newPack = Packages.AddNew();
						newPack.B5_MarksAndNumbers = pack.B5_MarksAndNumbers;
						newPack.B5_UnitType = pack.B5_UnitType;
						newPack.B5_UnitCount = pack.B5_UnitCount;
					}

					AdditionalInfos.RemoveAndDeleteAll();
					foreach (NctsAdditionalInfo addInfo in originalItem.AdditionalInfos)
					{
						var sgi = AdditionalInfos.AddNew();
						sgi.CSI_Code = addInfo.CSI_Code;
						sgi.CSI_Description = addInfo.CSI_Description;
					}
				}
			}
		}

		static ZString GetCommentsValue(ZString pointerToTheAttribute, ZString value, CusAddInfoCollection<ResultsOfControlAddInfo> collection)
		{
			var resultsOfControl = collection.OfType<CusAddInfo<ResultsOfControlAddInfo>>().FirstOrDefault(r => r.Data.G9_PointerToTheAttribute == pointerToTheAttribute);
			if (resultsOfControl == null)
			{
				resultsOfControl = collection.AddNew();
				resultsOfControl.Data.G9_PointerToTheAttribute = pointerToTheAttribute;
				resultsOfControl.Data.G9_Description = value;
			}
			return resultsOfControl.Data.G9_Description;
		}

		static void SetCommentsValue(ZString pointerToTheAttribute, ZString value, CusAddInfoCollection<ResultsOfControlAddInfo> collection)
		{
			var resultsOfControl = collection.OfType<CusAddInfo<ResultsOfControlAddInfo>>().FirstOrDefault(r => r.Data.G9_PointerToTheAttribute == pointerToTheAttribute);
			if (resultsOfControl == null)
			{
				resultsOfControl = collection.AddNew();
				resultsOfControl.Data.G9_PointerToTheAttribute = pointerToTheAttribute;
			}
			resultsOfControl.Data.G9_Description = value;
		}

		[ResourceStringData("EU.NCTS.NctsCommonCargoDesc|IsChecked", Caption = "Checked?", ShortCaption = "OK?")]
		public ZBool IsChecked
		{
			get => GetCheckItemValue(ItemCheckedPointer, false, ResultsOfControlCollection);
			set
			{
				var oldValue = IsChecked;
				SetCheckItemValue(ItemCheckedPointer, value, ResultsOfControlCollection);
				IsCheckedInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo IsCheckedInfo => GetZPropertyInfo(nameof(IsChecked));

		[ReadOnlyMember(nameof(IsNew))]
		[ResourceStringData("EU.NCTS.NctsCommonCargoDesc|IsDifferences", Caption = "Differences?")]
		public virtual ZBool HasDifferences
		{
			get => GetCheckItemValue(ResultOfControlCodes.Codes.Different, false, ResultsOfControlCollection);
			set
			{
				var oldValue = HasDifferences;
				SetCheckItemValue(ResultOfControlCodes.Codes.Different, value, ResultsOfControlCollection);
				if (HasDifferences != oldValue)
				{
					if (!value)
					{
						RestoreUnloadedItem();
					}
					ToggleUnloadedState();
				}
				HasDifferencesInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo HasDifferencesInfo => GetZPropertyInfo(nameof(HasDifferences));

		[ReadOnlyMember(nameof(IsNew))]
		[ResourceStringData("EU.NCTS.NctsCommonCargoDesc|IsMissing", Caption = "Missing?")]
		public virtual ZBool IsMissing
		{
			get => GetCheckItemValue(ResultOfControlCodes.Codes.Missing, false, ResultsOfControlCollection);
			set
			{
				var oldValue = IsMissing;
				SetCheckItemValue(ResultOfControlCodes.Codes.Missing, value, ResultsOfControlCollection);
				if (IsMissing != oldValue)
				{
					if (value)
					{
						ClearUnloadedItem();
					}
					else
					{
						RestoreUnloadedItem();
					}
					ToggleUnloadedState();
				}
				IsMissingInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo IsMissingInfo => GetZPropertyInfo(nameof(IsMissing));

		[ReadOnly(true)]
		[ResourceStringData("EU.NCTS.NctsCommonCargoDesc|IsNew", Caption = "New?")]
		public virtual ZBool IsNew
		{
			get => GetCheckItemValue(ResultOfControlCodes.Codes.New, false, ResultsOfControlCollection);
			set
			{
				var oldValue = IsNew;
				SetCheckItemValue(ResultOfControlCodes.Codes.New, value, ResultsOfControlCollection);
				if (IsNew != oldValue)
				{
					ToggleUnloadedState();
				}
				IsNewInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo IsNewInfo => GetZPropertyInfo(nameof(IsNew));

		[MaxLength(140)]
		[ResourceStringData("EU.NCTS.NctsCommonCargoDesc|UnloadingNotes", Caption = "Unloading Notes")]
		public virtual ZString UnloadingNotes
		{
			get => GetCommentsValue(ResultOfControlCodes.Codes.Other, ZString.Empty, ResultsOfControlCollection);
			set
			{
				var oldValue = UnloadingNotes;
				CheckMaximumLength(UnloadingNotesInfo, value);
				SetCommentsValue(ResultOfControlCodes.Codes.Other, value, ResultsOfControlCollection);
				UnloadingNotesInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo UnloadingNotesInfo => GetZPropertyInfo(nameof(UnloadingNotes));

		public bool UnloadedGoodsItemsReadOnly => MoveHeader.IsUnloadingMovementHeader && !HasDifferences && !IsNew;

		protected virtual void ToggleUnloadedState()
		{
			if (IsMissing)
			{
				if (HasDifferences)
				{
					HasDifferences = false;
				}
			}
			else if (HasDifferences)
			{
				if (IsMissing)
				{
					IsMissing = false;
				}
			}
			else if (IsNew)
			{
				if (IsMissing)
				{
					IsMissing = false;
				}
				if (HasDifferences)
				{
					HasDifferences = false;
				}
			}
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var newGoodsItem = (NctsArrivalAndUnloadingCargoDesc)base.CloneInternal(args);
			foreach (NctsContainer sourceContainer in Containers)
			{
				var newCont = (NctsContainer)newGoodsItem.Factory.New(sourceContainer.GetType());
				using (newCont.GetValidationSuspender())
				{
					newCont.BC_ParentID = newGoodsItem.PK;
					newCont.BC_ParentTableCode = newGoodsItem.TablePrefix;
					newCont.BC_ContainerNum = sourceContainer.BC_ContainerNum;
					newCont.BC_Seal1 = sourceContainer.BC_Seal1;
					newCont.BC_Seal2 = sourceContainer.BC_Seal2;
				}
			}
			return newGoodsItem;
		}

		protected override IEnumerable<IBusinessObjectFetchStrategy> GetAdditionalBusinessObjectFetchStrategies()
		{
			foreach (var additionalBusinessObjectFetchStrategy in base.GetAdditionalBusinessObjectFetchStrategies())
			{
				yield return additionalBusinessObjectFetchStrategy;
			}
			yield return new CusAddInfoTypeSupporterFetchStrategy(this);
		}

		protected override CusInBondCargoDescLookups GetNewLookups() => new NctsArrivalAndUnloadingCargoDescLookups(this);

		protected override CusInBondCargoDescValidation GetNewValidation() => new NctsArrivalAndUnloadingCargoDescValidation(this);

		IDictionary<ZString, Type> Integration.Customs.ICusAddInfoTypeSupporter.GetCusAddInfoTypes() => new Dictionary<ZString, Type>
		{
			{ CusAddInfoTypeAttribute.Codes.EuNctsResultsOfControl, typeof(CusAddInfo<ResultsOfControlAddInfo>) }
		};

		public override void OnSaving()
		{
			DeleteEmptySupportingDocuments();
			base.OnSaving();
		}

		void DeleteEmptySupportingDocuments()
		{
			foreach (NctsSupportingDocument supDoc in SupportingDocuments)
			{
				if (!supDoc.IsDeleted &&
					supDoc.CSI_ReferenceNumber.IsEmpty &&
					supDoc.CSI_ReferenceNumber2.IsEmpty &&
					supDoc.CSI_Code.IsEmpty &&
					supDoc.CSI_Status.IsEmpty)
				{
					supDoc.Delete();
				}
			}
		}

		public override void Delete()
		{
			if (!IsDeleted)
			{
				this.DeleteChildren<NctsContainer>(CusInBondContainerSchema.BC_ParentID);
			}
			base.Delete();
		}
	}
}
