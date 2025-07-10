using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public static class NctsHelper
	{
		public static bool IsUnloadedStateAccepted(string unloadedState)
		{
			return string.Equals(unloadedState, NctsUnloadedStateList.Codes.MIS, StringComparison.OrdinalIgnoreCase)
				|| string.Equals(unloadedState, NctsUnloadedStateList.Codes.DEC, StringComparison.OrdinalIgnoreCase)
				|| string.Equals(unloadedState, NctsUnloadedStateList.Codes.DIF, StringComparison.OrdinalIgnoreCase);
		}

		public static bool IsUnloadedStateDiscrepancy(ZString unloadedState)
		{
			switch (unloadedState)
			{
				case NctsUnloadedStateList.Codes.NEW:
				case NctsUnloadedStateList.Codes.MIS:
				case NctsUnloadedStateList.Codes.DIF:
					return true;
				default:
					return false;
			}
		}

		public static void RecalculateSequenceNoWhenAboutToBeDetachedOrDeleted(this INctsAdditionalInfoSequenceHeader header, IShortSequenceNumberLine line, string oldSubType)
		{
			switch (oldSubType)
			{
				case AdditionalInfoSubTypeList.Codes.AdditionalReference:
					header.RefSequenceNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(line);
					break;
				case AdditionalInfoSubTypeList.Codes.AdditionalInformation:
					header.InfSequenceNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(line);
					break;
				case AdditionalInfoSubTypeList.Codes.TransportDocument:
					header.TraSequenceNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(line);
					break;
			}
		}

		public static void RecalculateSequenceNoWhenAdded(this INctsAdditionalInfoSequenceHeader header, IShortSequenceNumberLine line, string subType)
		{
			switch (subType)
			{
				case AdditionalInfoSubTypeList.Codes.AdditionalReference:
					header.RefSequenceNumberGenerator.RecalculateWhenAdded(line);
					break;
				case AdditionalInfoSubTypeList.Codes.AdditionalInformation:
					header.InfSequenceNumberGenerator.RecalculateWhenAdded(line);
					break;
				case AdditionalInfoSubTypeList.Codes.TransportDocument:
					header.TraSequenceNumberGenerator.RecalculateWhenAdded(line);
					break;
			}
		}

		public static bool IsValidCountry(string country, BusinessObjectFactory factory)
		{
			return country.Length == StandardCountryCodeLength && factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, country) != null;
		}

		public static bool IsSecurityTypeBTHOrEXIOrENT(ZString typeOfSecurity)
		{
			return typeOfSecurity == NctsTypeOfSecurityList.Codes.BTH
					|| typeOfSecurity == NctsTypeOfSecurityList.Codes.ENT
					|| typeOfSecurity == NctsTypeOfSecurityList.Codes.EXI;
		}

		public static bool UnloadedStateInitiallyNew(ZPropertyInfo unloadedStateInfo) => (ZString)unloadedStateInfo.OriginalValue == NctsUnloadedStateList.Codes.NEW ;

		public const int StandardCountryCodeLength = 2;

		public static (bool Duplicate, ZString LRN) RetrieveLRNOfDeclarationWithMatchingMRN(ZGuid headerPK, ZString movementReferenceNumber, BusinessObjectFactory factory, ZString headerType)
		{
			var declarationReferenceWithDuplicateMRN = ZString.Empty;
			var isDuplicate = false;

			var cusInBondHeaderQuery = new ZDBOnlyQuery(typeof(CusInBondHeader));
			cusInBondHeaderQuery.AddToFilter(CusInBondHeaderSchema.BH_IsActive, true);
			cusInBondHeaderQuery.AddToFilter(CusInBondHeaderSchema.BH_HeaderType, headerType);
			cusInBondHeaderQuery.AddToFilter(CusInBondHeaderSchema.PK, SQLComparisonOperator.NotEqual, headerPK, ComparisonOptions.Default);
			var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusInBondHeader.Schema.TableName);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, movementReferenceNumber);
			cusInBondHeaderQuery.AddSubQuery(cusEntryNumQuery, JoinCondition.And);
			var queryResult = factory.LoadTop1<NctsHeader>(cusInBondHeaderQuery);

			if (queryResult != null)
			{
				declarationReferenceWithDuplicateMRN = queryResult.LocalReferenceNumber;
				isDuplicate = true;
			}

			return (isDuplicate, declarationReferenceWithDuplicateMRN);
		}

		internal static void RecalculateCustomsOfficeSequenceNoWhenAboutToBeDetachedOrDeleted(this NctsHeader header, IShortSequenceNumberLine line, ZString oldSubType)
		{
			header.OfficeSequenceNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(line, oldSubType);
		}

		internal static void RecalculateCustomsOfficeSequenceNoWhenAboutToBeDetachedOrDeleted(this NctsCommonMovementHeader movementHeader, IShortSequenceNumberLine line, ZString oldSubType)
		{
			movementHeader.OfficeSequenceNumberGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(line, oldSubType);
		}

		internal static void RecalculateCustomsOfficeSequenceNoWhenAdded(this NctsHeader header, IShortSequenceNumberLine line, ZString subType)
		{
			header.OfficeSequenceNumberGenerator.RecalculateWhenAdded(line, subType);
		}

		internal static void RecalculateCustomsOfficeSequenceNoWhenAdded(this NctsCommonMovementHeader movementHeader, IShortSequenceNumberLine line, ZString subType)
		{
			movementHeader.OfficeSequenceNumberGenerator.RecalculateWhenAdded(line, subType);
		}

		public static void RecalculateCustomsOfficeSequenceWhenRenumbered(this NctsHeader header, IShortSequenceNumberLine line, ZShort oldValue, ZString subType)
		{
			header.OfficeSequenceNumberGenerator.RecalculateWhenRenumbered(line, oldValue, subType);
		}

		public static void RecalculateCustomsOfficeSequenceWhenRenumbered(this NctsCommonMovementHeader movementHeader, IShortSequenceNumberLine line, ZShort oldValue, ZString subType)
		{
			movementHeader.OfficeSequenceNumberGenerator.RecalculateWhenRenumbered(line, oldValue, subType);
		}

		public static bool IsNCTSPreviousDocument(ZString code) => code.StartsWith("N");

		public static void SetAllUnloadedStateToDEC(this IUnloadedStatusSupporter supporter)
		{
			var itemsToDelete = new List<IUnloadedStatusSupporter>();
			foreach (var item in supporter.RelatedItems)
			{
				if (IsUnloadedStateAccepted(item.UnloadedStatus))
				{
					item.SetAllUnloadedStateToDEC();
				}
				else if (item.UnloadedStatus == NctsUnloadedStateList.Codes.NEW)
				{
					itemsToDelete.Add(item);
				}
			}
			itemsToDelete.ForEach(x => x.Delete());

			supporter.UnloadedStatus = NctsUnloadedStateList.Codes.DEC;
		}

		public static bool Has30600AdditionalInformation(this IEnumerable<AdditionalInfo> additionalInfo)
		{
			return additionalInfo.Any(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation && x.CSI_Code == AdditionalDocumentTypes._30600);
		}

		internal static bool HasNonDECUnloadedItem(this IUnloadedStatusSupporter supporter)
		{
			return supporter.UnloadedStatus != NctsUnloadedStateList.Codes.DEC || supporter.RelatedItems.Any(x => x.HasNonDECUnloadedItem());
		}

		public static CodeDescriptionPairList GetCachedNctsBillAdditionalDocumentStatusList(BusinessObjectFactory factory, bool removeNewCode)
		{
			return factory.GetCachedValue($"EU.NCTS.NctsAdditionalInfo.StatusList_isNew{removeNewCode}", () =>
			{
				var list = new NctsBillAdditionalDocumentStatusList();
				if (removeNewCode)
				{
					list.RemoveCode(NctsBillAdditionalDocumentStatusList.Codes.NEW);
				}

				return list;
			});
		}

		public static bool ShowExportTransportModeDetails(NctsDepartureMovementHeader header)
		{
			return (header is not null) && (string)header.BM_ExportTransportMode switch
			{
				ModeOfTransportList.Codes._1_SeaTransport
					or ModeOfTransportList.Codes._2_RailTransport
					or ModeOfTransportList.Codes._3_RoadTransport
					or ModeOfTransportList.Codes._4_AirTransport
					or ModeOfTransportList.Codes._7_FixedTransportInstallations
					or ModeOfTransportList.Codes._8_InlandWaterwayTransport
					or ModeOfTransportList.Codes._9_OwnPropulsion => true,
				_ => false
			};
		}
	}

	public interface IUnloadedStatusSupporter
	{
		ZString UnloadedStatus { get; set; }
		void Delete();
		IEnumerable<IUnloadedStatusSupporter> RelatedItems { get; }
	}
}
