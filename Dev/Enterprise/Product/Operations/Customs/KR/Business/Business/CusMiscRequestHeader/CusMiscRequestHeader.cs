using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class CusMiscRequestHeader : Customs.Business.CusMiscRequestHeader, IControllerIDProvider, IDocumentSupportable, IEDIMessageCollectionProviderWithID, ISupportMultipleResourceStringData
	{
		public CusMiscRequestHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		public new CusMiscRequestLineCollection RequestLines => (CusMiscRequestLineCollection)base.RequestLines;
		protected override Customs.Business.CusMiscRequestLineCollection GetRequestLines() => new CusMiscRequestLineCollection(this);
		protected override ZString HumanReadableNameCore => Res.GetString("FFF41FF5-4C48-4D71-BDB7-CBC93A75F041", "Request {0}", CMR_JobNumber.TrimEnd());

		#region Property
		[ResourceStringData("618A1A42-902A-49CD-A102-AF59F3AB3C96", Caption = "Message Type")]
		[List(nameof(Lookups) + "." + nameof(CusMiscRequestHeaderLookups.MessageTypeList))]
		public override ZString CMR_MessageType
		{
			get => base.CMR_MessageType;
			set => base.CMR_MessageType = value;
		}

		[ResourceStringData("6C920C40-66F1-49F3-AEA6-9C03C8065709", Caption = "Branch Code")]
		public override ZGuid CMR_GB
		{
			get => base.CMR_GB;
			set => base.CMR_GB = value;
		}

		[ResourceStringData("0244E978-209F-4A1F-B86A-C2D207DD3B18", Caption = "Request Details")]
		public override ZString CMR_RequestDetails
		{
			get => base.CMR_RequestDetails;
			set => base.CMR_RequestDetails = value;
		}

		[ResourceStringData("A8169F0D-C0F7-48E2-B941-0CE68B39533B", Caption = "Status")]
		[List(nameof(Lookups) + "." + nameof(CusMiscRequestHeaderLookups.CustomsMessageStatusTypeList))]
		public override ZString CMR_Status
		{
			get => base.CMR_Status;
			set => base.CMR_Status = value;
		}

		[ResourceStringData("8976DE3E-06E2-48C7-9C4E-1F5C71766D74", Caption = "Request Date")]
		public override ZDateTime CMR_RequestDate
		{
			get => base.CMR_RequestDate;
			set => base.CMR_RequestDate = value;
		}

		[ResourceStringData("2E8B5EEB-D40D-451A-B15C-053ED94DF81C", Caption = "Customs Office")]
		[List(nameof(Lookups) + "." + nameof(CusMiscRequestHeaderLookups.CustomsOfficeList))]
		public ZString CustomsOffice
		{
			get
			{
				var result = ZString.Empty;
				switch (CMR_MessageType)
				{
					case ElectronicDocumentTypeList.Codes._5SG:
						result = CMR_CustomsOffice;
						break;
					case ElectronicDocumentTypeList.Codes._5AC:
					case ElectronicDocumentTypeList.Codes._5GW:
						result = CMR_CustomsOffice.Length == 5 ? CMR_CustomsOffice.Substring(0, 3) : ZString.Empty;
						break;
				}
				return result;
			}
		}

		public ZPropertyInfo CustomsOfficeInfo => GetZPropertyInfo(nameof(CustomsOffice));

		[ResourceStringData("C0991C20-FB7E-4AC6-8637-A54B4CBC7469", Caption = "Department")]
		[List(nameof(Lookups) + "." + nameof(CusMiscRequestHeaderLookups.CustomsDivisionList))]
		public ZString CustomsDivision => CMR_CustomsOffice.Length == 5 ? CMR_CustomsOffice.Substring(3, 2) : ZString.Empty;
		public ZPropertyInfo CustomsDivisionInfo => GetZPropertyInfo(nameof(CustomsDivision));

		[ResourceStringData("59F58F02-3A21-4E86-86C5-290D351D0EDB", Caption = "Entry Count")]
		public ZInt LinesCount => RequestLines.Count;
		public ZPropertyInfo LinesCountInfo => GetZPropertyInfo(nameof(LinesCount));

		[ResourceStringData("24E6D9F6-77CC-4CA7-82F7-51BDF998AB2A", Caption = "Application Number")]
		public ZString FormattedApplicationNumber
		{
			get
			{
				var result = ZString.Empty;
				if (CusEntryNumber != null)
				{
					switch (CMR_MessageType)
					{
						case ElectronicDocumentTypeList.Codes._5SG:
							result = MessageFunctions.GetFormattedNumber(CusEntryNumber.CE_EntryNum, new int[] { 0, 3, 8, 12, 13 });
							break;
						case ElectronicDocumentTypeList.Codes._5AC:
						case ElectronicDocumentTypeList.Codes._5GW:
							result = MessageFunctions.DeclarationNumberFormat(CusEntryNumber.CE_EntryNum);
							break;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo FormattedApplicationNumberInfo => GetZPropertyInfo(nameof(FormattedApplicationNumber));

		[ResourceStringData("10E5257F-D6DD-4D91-A61E-0F6D4004AA7C", Caption = "Message Type Name")]
		public ZString MessageTypeName => Lookups.MessageTypeList.GetDescriptionFromCode(CMR_MessageType);

		[ResourceStringData("8701A238-EE6A-43BC-A430-4CF55DEAA6B3", Caption = "Status Name")]
		public ZString StatusName => Lookups.CustomsMessageStatusTypeList.GetDescriptionFromCode(CMR_Status);

		[ResourceStringData("F83F8BEB-EACC-458D-B2D4-8ACA7C43D4C7", Caption = "Customs Office Name")]
		public ZString CustomsOfficeName
		{
			get
			{
				var result = ZString.Empty;
				var customsOffice = GetRefCusCodeData(Factory, CustomsOffice, ZZ.NKCodeType.CustomsOffice)?.ZZD_Description ?? ZString.Empty;
				var customsDepartment = GetRefCusCodeData(Factory, CustomsDivision, ZZ.NKCodeType.CustomsDepartment)?.ZZD_Description ?? ZString.Empty;

				if (!customsOffice.IsEmpty && !customsDepartment.IsEmpty)
				{
					result = customsOffice + " " + customsDepartment;
				}
				else
				{
					result = customsOffice + customsDepartment;
				}
				return result;
			}
		}

		[ResourceStringData("D167E6DE-2AFA-4146-A61A-099D74EB94ED", Caption = "Declarant Company Name")]
		public ZString DeclarantCompanyName => Branch?.Company?.GC_Name ?? ZString.Empty;

		[ResourceStringData("5E276E2F-E493-4A82-9476-F8991FB30E87", Caption = "Broker Name")]
		public ZString BrokerName => Broker?.GS_FullName ?? ZString.Empty;

		[ResourceStringData("DE747300-28A4-4CBC-8237-C77FB9647529", Caption = "Customs Office")]
		public ZString FormattedCustomsOffice
		{
			get
			{
				var result = CMR_CustomsOffice;
				if (CMR_CustomsOffice.Length == 5)
				{
					result = CMR_CustomsOffice.Substring(0, 3) + "-" +
							 CMR_CustomsOffice.Substring(3);
				}
				return result;
			}
		}

		[ResourceStringData("CE995E1A-5C90-4F68-879C-CAB8B1693D16", Caption = "Request Details")]
		public ZString FormattedRequestDetails => CMR_RequestDetails.Replace("\r\n", "");
		#endregion

		static Universal.ZZRefCusCodeListCombined GetRefCusCodeData(BusinessObjectFactory factory, ZString code, ZString type)
		{
			return MessageFunctions.GetRefCusCodeList(factory, code, type);
		}

		protected override Customs.Business.CusMiscRequestHeaderLookups GetNewLookups() => new CusMiscRequestHeaderLookups(this);
		public new CusMiscRequestHeaderLookups Lookups => (CusMiscRequestHeaderLookups)base.Lookups;

		#region CusEntryNumber
		public CusEntryNumber CusEntryNumber
		{
			get
			{
				if (cusEntryNumber == null)
				{
					cusEntryNumber = LoadCusEntryNumber();
				}
				return cusEntryNumber;
			}
		}
		CusEntryNumber cusEntryNumber;

		CusEntryNumber LoadCusEntryNumber()
		{
			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, PK);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, CMR_MessageType);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Enterprise.Core.Constants.CountryCodes.KoreaSouth);
			query.OrderBy = CusEntryNumSchema.CE_SystemCreateTimeUtc.Name;

			return Factory.LoadTop1<CusEntryNumber>(query);
		}
		public CusEntryNumber CreateCusEntryNumber()
		{
			var result = Factory.New<CusEntryNumber>();
			result.CE_EntryIsSystemGenerated = true;
			result.CE_ParentID = PK;
			result.CE_ParentTable = TableName;
			result.CE_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.KoreaSouth;
			result.CE_EntryType = CMR_MessageType;
			return result;
		}
		#endregion
		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsInDatabase)
			{
				if (CMR_JobNumber.IsEmpty)
				{
					var seed = Enterprise.Core.Constants.CountryCodes.KoreaSouth + CusMiscRequestHeaderSchema.Constants.Prefix;
					var fountainNumber = Env.NumberFountains.KRNumberFountain(seed, NumberFountainMaxValues._8digit).GetNextFormatted(Factory);
					CMR_JobNumber = Constants.CusMiscRequestJobNumberPrefix + ZInt.ParseSafe(fountainNumber, 0).ToString(Constants.NumberFormatDigit.D8);
				}
				SaveCusEntryNumber();
			}
		}
		public void SaveCusEntryNumber()
		{
			if (CusEntryNumber == null)
			{
				cusEntryNumber = CreateCusEntryNumber();
			}
			if (CusEntryNumber.CE_EntryNum.IsEmpty)
			{
				CusEntryNumber.CE_EntryType = CMR_MessageType;
				if (Branch != null)
				{
					ZString uniPassDeclarantID = KRCustomsRegistry.Instance.UNIPASSDeclarantID.GetValueWithFallbackDefault(Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty)?.ToString() ?? ZString.Empty;
					if (!uniPassDeclarantID.IsEmpty)
					{
						var seed = Enterprise.Core.Constants.CountryCodes.KoreaSouth + CMR_MessageType;
						var year4digit = ZDate.Today.ToString(Constants.DateFormatType.Year);
						var year2digit = year4digit.Substring(2, 2);
						var fountainNumber = ZString.Empty;
						switch (CMR_MessageType)
						{
							case ElectronicDocumentTypeList.Codes._5SG:
								fountainNumber = Env.NumberFountains.KREntryNumberFountain(seed, uniPassDeclarantID, year4digit, NumberFountainMaxValues._6digit).GetNextFormatted(Factory);
								CusEntryNumber.CE_EntryNum = CMR_MessageType + uniPassDeclarantID + year4digit + Constants.EntryNumberCheckDigit.X + ZInt.ParseSafe(fountainNumber, 0).ToString(Constants.NumberFormatDigit.D6);
								break;
							case ElectronicDocumentTypeList.Codes._5AC:
							case ElectronicDocumentTypeList.Codes._5GW:
								fountainNumber = Env.NumberFountains.KREntryNumberFountain(seed, uniPassDeclarantID, year2digit, NumberFountainMaxValues._7digit).GetNextFormatted(Factory);
								CusEntryNumber.CE_EntryNum = uniPassDeclarantID + year2digit + ZInt.ParseSafe(fountainNumber, 0).ToString(Constants.NumberFormatDigit.D7) + Constants.EntryNumberCheckDigit.U;
								break;
						}
					}
				}
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded && !IsInDatabase)
			{
				CMR_JobNumber = ZString.Empty;
				if (CusEntryNumber != null)
				{
					CusEntryNumber.CE_EntryNum = ZString.Empty;
				}
			}
		}

		[ChildEditable(true)]
		public EDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new EDIMessageCollection(this);
					messages.Load();
					RegisterEditableChildObject(messages);
				}
				return messages;
			}
		}
		EDIMessageCollection messages;

		ControllerID IControllerIDProvider.ControllerID => ControllerIDs.Customs.KR.MiscRequestMessages;

		Guid IControllerIDProvider.BusinessObjectPK => PK.ToGuid();

		Enterprise.Messaging.Business.EDIMessageCollection IEDIMessageCollectionProvider.Messages => Messages;

		BusinessObjectFactory IEDIMessageCollectionProvider.Factory => Factory;

		DocumentSupporter IDocumentSupportable.DocumentSupporter => documentSupporter ?? (documentSupporter = new CusMiscRequestHeaderDocumentSupporter(this));
		DocumentSupporter documentSupporter;

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new CusMiscRequestHeaderFetchStrategy(this);

		ZString IEDIMessageCollectionProviderWithID.IDNumber => CusEntryNumber?.CE_EntryNum ?? ZString.Empty;
		void IEDIMessageCollectionProviderWithID.MarkAsFailed()
		{
			var failedStatus = CustomsMessageStatusTypeList.GetErrorStatus(CMR_Status);
			if (!string.IsNullOrEmpty(failedStatus))
			{
				CMR_Status = failedStatus;
			}
		}

		public IReadOnlyList<string> MultipleKeysToUse => new string[] { CMR_MessageType };

		[ResourceStringData("985A8DAD-B87F-4DAD-B39D-C66CAC398ED3", Caption = "Application Start Period")]
		public ZDateTime ApplicationStartPeriod
		{
			get
			{
				var result = ZDateTime.Empty;
				if (CMR_MessageType == ElectronicDocumentTypeList.Codes._5AC || CMR_MessageType == ElectronicDocumentTypeList.Codes._5GW)
				{
					result = CusEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
				}
				return result;
			}
		}

		[ResourceStringData("B6A5705D-4D48-4491-A794-B2E9C5999DE3", Caption = "Customs Review Status")]
		[List(nameof(Lookups) + "." + nameof(CusMiscRequestHeaderLookups.CustomsEntryStatusTypeList))]
		public ZString CustomsReviewStatus
		{
			get
			{
				if (!customsReviewStatus.HasValue)
				{
					var message5SG = Messages.Cast<EDIMessage>().Where(x => x.EM_MessageType == ElectronicDocumentTypeList.Codes._5SG).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
					customsReviewStatus = message5SG?.MessageOrEntryStatus ?? ZString.Empty;
				}
				return customsReviewStatus.Value;
			}
		}
		ZString? customsReviewStatus;

		[ResourceStringData("5577281F-58C1-4AF1-8024-394C154241BD", Caption = "Review Date (5SG)")]
		public ZDateTime ReviewDate5SG
		{
			get
			{
				var result = ZDateTime.Empty;
				if (CMR_MessageType == ElectronicDocumentTypeList.Codes._5SG)
				{
					result = CusEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
				}
				return result;
			}
		}
	}
}
