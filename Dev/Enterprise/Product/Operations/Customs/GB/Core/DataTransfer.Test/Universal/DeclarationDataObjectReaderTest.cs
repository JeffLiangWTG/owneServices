using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using EntryNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.EntryNumber;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.GB.DataTransfer.Universal.Testing
{
	class DeclarationDataObjectReaderTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		[TestDate(2019, 11, 11)]
		public void TestFillDeclarationWithSuspendedSettersPropertiesForChief()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Port, "port");
			var codelist1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Port, "DEU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(codelist1.PK, "Type", "CA3");
			Factory.SaveForTesting();

			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import },
					WayBillNumber = "MASTERDEFER",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
					PortOfDischarge = new UNLOCO() { Code = "GBBLE", Name = "Bletchley" },
					MessagingApplicationCode = new CodeDescriptionPair() { Code = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF },
				};
				declarationDataObject.SetEntryInstructionCollection(() => new List<EntryInstruction>()
					{
						new EntryInstruction()
						{
							Style = "AAA",
							SubStyle = new CodeDescriptionPair() { Code = "S", Description = "Sub Style" }
						}
					});
				declarationDataObject.SetDateCollection(() => new List<Date>()
					{
						new Date()
						{
							Type = DateType.Arrival,
							IsEstimate = true,
							Value = new ZDateTime(2019, 11, 23)
						}
					});

				var logger = new TestErrorLogger();
				var provider = new CustomsShipmentDataObjectReaderProvider();
				BusinessObject bizObj = null;

				CombineAssertions(() =>
				{
					AssertExceptionThrown<MessageProcessingBusinessFailureException>("Not saved if application code is CHF in XML for new declaration", "Save aborted because this is a CHIEF declaration", () => provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj));
					AssertNull(bizObj);
				});
			}
		}

		[TestDate(2019, 11, 11)]
		public void TestFillDeclarationWithSuspendedSettersPropertiesForCDS()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory.BOFactory);
			helper.CreateCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Port, "port");
			var codelist1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Port, "DEU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(codelist1.PK, "Type", "CA3");
			Factory.SaveForTesting();

			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import },
					WayBillNumber = "MASTERDEFER",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
					PortOfDischarge = new UNLOCO() { Code = "GBBLE", Name = "Bletchley" },
					MessagingApplicationCode = new CodeDescriptionPair() { Code = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services },
				};
				declarationDataObject.SetEntryInstructionCollection(() => new List<EntryInstruction>()
					{
						new EntryInstruction()
						{
							Style = "AAA",
							SubStyle = new CodeDescriptionPair() { Code = "S", Description = "Sub Style" }
						}
					});
				declarationDataObject.SetDateCollection(() => new List<Date>()
					{
						new Date()
						{
							Type = DateType.Arrival,
							IsEstimate = true,
							Value = new ZDateTime(2019, 11, 23)
						}
					});

				var logger = new TestErrorLogger();
				var provider = new CustomsShipmentDataObjectReaderProvider();
				BusinessObject bizObj = null;
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);

				var declarationBO = (JobDeclaration)bizObj;
				AssertEquals("", declarationBO.JE_LocationOfGoods);
				AssertEquals("", declarationBO.JE_SubLocationOfGoods);
				AssertEquals("CDS", declarationBO.JE_ApplicationCode);
				AssertEquals(ZDateTime.Empty, declarationBO.JE_EntryAuthorisationDate);

				declarationDataObject.LocationAtClearance = new CodeDescriptionPair35Char() { Code = "AaBbCcDddddddddddddddEee" };
				declarationDataObject.SubLocationAtClearance = new CodeDescriptionPair35Char() { Code = "SUB" };
				declarationDataObject.DateCollection.Add(new Date()
				{
					Type = DateType.EntryAuthorisation,
					IsEstimate = false,
					Value = new ZDateTime(2019, 11, 24)
				});

				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
				declarationBO = (JobDeclaration)bizObj;
				AssertEquals("", declarationBO.JE_LocationOfGoods);
				AssertEquals("Ddddddddddddddd", declarationBO.JE_GoodsLocation);
				AssertEquals("Aa", declarationBO.JE_Calc_LocationOtherInformationCountry);
				AssertEquals("Bb", declarationBO.JE_Calc_LocationOtherInformationType);
				AssertEquals("Cc", declarationBO.JE_LocationQualifier);
				AssertEquals("SUB", declarationBO.JE_SubLocationOfGoods);
				AssertEquals(new ZDateTime(2019, 11, 24), declarationBO.JE_EntryAuthorisationDate);
			}
		}

		public void TestFillMasterUCRFromEntryNumberCollectionForChief()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CHF";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.SaveForTesting();

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
			dataContext.CodesMappedToTarget = true;
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, declaration.JE_DeclarationReference);

			var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				MessageType = new CodeDescriptionPair { Code = JobMessageTypeList.Codes.Import },
				WayBillNumber = "MASTERDEFER",
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				PortOfDischarge = new UNLOCO() { Code = "GBBLE", Name = "Bletchley" },
				MessagingApplicationCode = new CodeDescriptionPair
				{
					Code = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF
				}
			};
			declarationDataObject.SetEntryNumberCollection(() => new List<EntryNumber>()
			{
				new EntryNumber
				{
					Type = new EntryType { Code = CusEntryNumberTypes.EU.MasterUCR },
					Number = "TEST-123456"
				}
			});

			var logger = new TestErrorLogger();
			var reader1 = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO1 = (JobDeclaration)reader1.ReadIntoBusinessObject();
			AssertNullOrEmpty(declarationBO1.JE_MasterUCR);

			CombineAssertions(() =>
			{
				var provider = new CustomsShipmentDataObjectReaderProvider();
				BusinessObject bizObj = null;
				AssertExceptionThrown<MessageProcessingBusinessFailureException>("Not saved if the existing declaration's application code is CHF", "Save aborted because this is a CHIEF declaration", () => provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj));
				AssertNull(bizObj);
				var declarationInDB = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.PK, declaration.PK));
				AssertNullOrEmpty(declarationInDB.JE_MasterUCR);
			});
		}

		public void TestSuspendApplicationCodeChangeOnRead()
		{
			var badgeCdsAbc = new BadgeCodeSetting() { CSPCode = GatewayList.Codes.CDS, BadgeCode = "ABC", ApplicationCode = "CHF" };
			var badgeCdsDef = new BadgeCodeSetting() { CSPCode = GatewayList.Codes.CDS, BadgeCode = "DEF", ApplicationCode = "CDS" };
			var badgeCollection = new BadgeCodeSettingCollection();
			badgeCollection.AddRange(badgeCdsAbc, badgeCdsDef);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCollection);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair { Code = JobMessageTypeList.Codes.Import },
					WayBillNumber = "MASTERDEFER",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
					PortOfDischarge = new UNLOCO() { Code = "GBBLE", Name = "Bletchley" },
					MessagingApplicationCode = new CodeDescriptionPair
					{
						Code = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services
					},
					CustomsProfileIdentifier = new ValueTypePair
					{
						Value = "ABC"
					},
					PortOfFirstArrival = new UNLOCO() { Code = "" }
				};

				var logger = new TestErrorLogger();
				var provider = new CustomsShipmentDataObjectReaderProvider();
				var reader = provider.GetReader(declarationDataObject, logger, Factory, null);

				BusinessObject bizObj = null;
				reader.ReadIntoBusinessObject(ref bizObj);
				var declarationBO1 = (JobDeclaration)bizObj;
				AssertEquals("CDS", declarationBO1.JE_ApplicationCode);
				AssertEquals("ABC", declarationBO1.JE_CustomsProfile);

				var declaration1 = Factory.New<JobDeclaration>();
				declaration1.JE_ApplicationCode = "CHF";
				declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration1.JE_CustomsProfile = "ABC";
				bizObj = declaration1;
				reader.ReadIntoBusinessObject(ref bizObj);
				AssertEquals("CDS", declarationBO1.JE_ApplicationCode);
				AssertEquals("ABC", declarationBO1.JE_CustomsProfile);

				declarationDataObject.MessagingApplicationCode = null;

				CombineAssertions(() =>
				{
					bizObj = null;
					AssertExceptionThrown<MessageProcessingBusinessFailureException>("Not saved if default application code is CHF for new declaration", "Save aborted because this is a CHIEF declaration", () => reader.ReadIntoBusinessObject(ref bizObj));
					AssertNull(bizObj);
					var declarationInDB = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.PK, declaration1.PK));
					AssertEquals("CDS", declarationInDB.JE_ApplicationCode);
					AssertEquals("ABC", declarationInDB.JE_CustomsProfile);
				});

				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.JE_ApplicationCode = "CDS";
				declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration2.JE_CustomsProfile = "DEF";
				bizObj = declaration2;
				CombineAssertions(() =>
				{
					AssertExceptionThrown<MessageProcessingBusinessFailureException>("Not saved if default application code is CHF for existing declaration", "Save aborted because this is a CHIEF declaration", () => reader.ReadIntoBusinessObject(ref bizObj));
					declaration2 = (JobDeclaration)bizObj;
					AssertEquals("CDS", declaration2.JE_ApplicationCode);
					AssertEquals("DEF", declaration2.JE_CustomsProfile);
				});
			}
		}

		public void TestFillExistingDeclarationWithEmptyMessagingApplicationCodeInUSXML()
		{
			var badgeCdsAbc = new BadgeCodeSetting() { CSPCode = GatewayList.Codes.CDS, BadgeCode = "ABC", ApplicationCode = "CHF" };
			var badgeCdsDef = new BadgeCodeSetting() { CSPCode = GatewayList.Codes.CDS, BadgeCode = "DEF", ApplicationCode = "CDS" };
			var badgeCollection = new BadgeCodeSettingCollection();
			badgeCollection.AddRange(badgeCdsAbc, badgeCdsDef);
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCollection);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair { Code = JobMessageTypeList.Codes.Import },
					WayBillNumber = "MASTERDEFER",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
					PortOfDischarge = new UNLOCO() { Code = "GBBLE", Name = "Bletchley" },
					MessagingApplicationCode = new CodeDescriptionPair
					{
						Code = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services
					},
					CustomsProfileIdentifier = new ValueTypePair
					{
						Value = "DEF"
					},
					PortOfFirstArrival = new UNLOCO() { Code = "" }
				};

				var logger = new TestErrorLogger();
				var provider = new CustomsShipmentDataObjectReaderProvider();
				var reader = provider.GetReader(declarationDataObject, logger, Factory, null);

				BusinessObject bizObj = null;
				reader.ReadIntoBusinessObject(ref bizObj);
				var declarationBO1 = (JobDeclaration)bizObj;
				AssertEquals("CDS", declarationBO1.JE_ApplicationCode);
				AssertEquals("DEF", declarationBO1.JE_CustomsProfile);

				Factory.SaveForTesting();

				declarationDataObject.MessagingApplicationCode = null;
				declarationDataObject.CustomsProfileIdentifier = new ValueTypePair
				{
					Value = "ABC"
				};

				CombineAssertions(() =>
				{
					AssertExceptionThrown<MessageProcessingBusinessFailureException>("Not saved if default application code is CHF for existing declaration", "Save aborted because this is a CHIEF declaration", () => reader.ReadIntoBusinessObject(ref bizObj));
					var declaration2 = (JobDeclaration)bizObj;
					AssertEquals("CDS", declaration2.JE_ApplicationCode);
					AssertEquals("DEF", declaration2.JE_CustomsProfile);
				});
			}
		}
	}
}
