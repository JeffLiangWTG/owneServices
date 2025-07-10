using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Integration.Test;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors
{
	public static class SetupTestData
	{
		public static void ForCommonCase(TestConfiguration config)
		{
			var factory = new BusinessObjectFactory();

			var accountingTestDataCreator = ObjectFactory.Get<IAccountingTestDataCreator>();
			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();

			accountingTestDataCreator.CreatePeriods(200701, new ZDateTime(2007, 1, 1), new ZDateTime(2007, 1, 31), config.IsPeriodClosed);
			factory.Save();

			if (config.NeedTableRecordCounts)
			{
				config.TableRecordCountsBaseline = TableRecordCountsHelper.GetTableRecordCounts();
			}

			forwardingTestDataCreator.CreateConsolData(3, 0, out var consol1PK, config.IsForwardingDataCancelled, out var shipmentPKs, out var outPutindex);//3
			config.Consol1PK = consol1PK;
			config.ShipmentPKs = shipmentPKs;

			var jobHeaderShipment1 = factory.NewJobForTesting<JobHeader>();
			jobHeaderShipment1.JH_JobNum = "1111";
			jobHeaderShipment1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeaderShipment1.JH_ParentID = shipmentPKs[0];

			var jobHeaderShipment2 = factory.NewJobForTesting<JobHeader>();
			jobHeaderShipment2.JH_JobNum = "2222";
			jobHeaderShipment2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeaderShipment2.JH_ParentID = shipmentPKs[1];

			factory.Save();

			var postDate = new ZDateTime(2007, 1, 14);
			var jobClosedDate = new ZDateTime(2007, 1, 20);
			if (config.NeedJobHeaderConsol)
			{
				var jobHeaderConsol1 = factory.NewJobForTesting<JobHeader>();
				jobHeaderConsol1.JH_JobNum = "3333";
				jobHeaderConsol1.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
				jobHeaderConsol1.JH_ParentID = consol1PK;
				jobHeaderConsol1.JH_A_JCL = jobClosedDate;
				factory.Save();

				accountingTestDataCreator.CreateConsolAccountingData(jobHeaderConsol1.PK, consol1PK, postDate, config.IsHotChequeCancelled, config.IsHotChequeLinkedToAH);
				jobHeaderConsol1.JH_Status = JobHeaderStatus.Closed.Code;
				factory.Save();
			}

			accountingTestDataCreator.CreateAccountingData(jobHeaderShipment1.PK, postDate, config.IsHotChequeCancelled, config.IsHotChequeLinkedToAH);
			accountingTestDataCreator.CreateAccountingData(jobHeaderShipment2.PK, postDate, config.IsHotChequeCancelled, config.IsHotChequeLinkedToAH);

			if (config.NeedWorkingStatusJobHeader)
			{
				forwardingTestDataCreator.CreateConsolData(4, outPutindex + 1, out var consol2PK, isCancelled: false, out var shipment2PKs, out _);//3

				var jobHeaderShipment3 = factory.NewJobForTesting<JobHeader>();
				jobHeaderShipment3.JH_JobNum = "5555";
				jobHeaderShipment3.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				jobHeaderShipment3.JH_ParentID = shipment2PKs[0];
				factory.Save();

				accountingTestDataCreator.CreateAccountingData(jobHeaderShipment3.PK, postDate, config.IsHotChequeCancelled, config.IsHotChequeLinkedToAH);

				jobHeaderShipment3.JH_Status = JobHeaderStatus.Closed.Code;
				jobHeaderShipment3.JH_A_JCL = jobClosedDate;

				var jobHeaderConsol2 = factory.NewJobForTesting<JobHeader>();
				jobHeaderConsol2.JH_JobNum = "8888";
				jobHeaderConsol2.JH_ParentTableCode = JobConsolSchema.Constants.Prefix;
				jobHeaderConsol2.JH_ParentID = consol2PK;
				jobHeaderConsol2.JH_A_JCL = jobClosedDate;
				factory.Save();

				accountingTestDataCreator.CreateConsolAccountingData(jobHeaderConsol2.PK, consol2PK, postDate, config.IsHotChequeCancelled, config.IsHotChequeLinkedToAH);
				jobHeaderConsol2.JH_Status = JobHeaderStatus.Closed.Code;
				factory.Save();

				forwardingTestDataCreator.CreateShipmentData(out var x);
				var companyFilter = new ZQuery(GlbCompanySchema.GC_Code, "SIN");
				_ = companyFilter.AddToFilter(new ZQuery(GlbCompanySchema.GC_IsActive, true));
				var companiesByOrgProxy = factory.Load<GlbCompany>(companyFilter);

				var jobHeaderShipment1WorkingStatus = factory.NewJobForTesting<JobHeader>();
				jobHeaderShipment1WorkingStatus.JH_JobNum = "7777";
				jobHeaderShipment1WorkingStatus.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				jobHeaderShipment1WorkingStatus.JH_ParentID = x;
				factory.Save();

				accountingTestDataCreator.CreateAccountingData(jobHeaderShipment1WorkingStatus.PK, postDate, config.IsHotChequeCancelled, config.IsHotChequeLinkedToAH);
				jobHeaderShipment1WorkingStatus.JH_Status = JobHeaderStatus.Closed.Code;
				factory.Save();

				var jobHeaderShipment2WorkingStatus = factory.NewJobForTesting<JobHeader>();
				jobHeaderShipment2WorkingStatus.JH_GC = companiesByOrgProxy[0].PK;
				jobHeaderShipment2WorkingStatus.JH_JobNum = jobHeaderShipment1WorkingStatus.JH_JobNum;
				jobHeaderShipment2WorkingStatus.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				jobHeaderShipment2WorkingStatus.JH_ParentID = x;
				factory.Save();

				accountingTestDataCreator.CreateAccountingData(jobHeaderShipment2WorkingStatus.PK, postDate, config.IsHotChequeCancelled, config.IsHotChequeLinkedToAH);
				jobHeaderShipment2WorkingStatus.JH_Status = JobHeaderStatus.Working.Code;
				factory.Save();
			}

			jobHeaderShipment1.JH_Status = JobHeaderStatus.Closed.Code;
			jobHeaderShipment1.JH_A_JCL = jobClosedDate;

			if (config.AllJobHeadersNeedToBeClosed)
			{
				jobHeaderShipment2.JH_Status = JobHeaderStatus.Closed.Code;
				jobHeaderShipment2.JH_A_JCL = jobClosedDate;
			}

			if (config.NeedShipmentDeclaration)
			{
				var customsAttachedDeclarationDataCreator = GetCustomsAttachedDeclarationDataCreator(Env.CurrentCompany.Country.Code);
				if (customsAttachedDeclarationDataCreator != null)
				{
					foreach (var shipmentPK in config.ShipmentPKs)
					{
						_ = customsAttachedDeclarationDataCreator.CreateAttachedDeclarationData(shipmentPK, config.IsForwardingDataCancelled);
					}
				}
				else
				{
					var customsTestDataCreator = ObjectFactory.Get<ICustomsTestDataCreator>();
					foreach (var shipmentPK in config.ShipmentPKs)
					{
						_ = customsTestDataCreator.CreateAttachedDeclarationData(shipmentPK, config.IsForwardingDataCancelled);
						if (config.NeedJobHeaderCartage)
						{
							var jobHeaderCartage1 = factory.NewJobForTesting<JobHeader>();
							jobHeaderCartage1.JH_JobNum = "4444";
							jobHeaderCartage1.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;
							jobHeaderCartage1.JH_ParentID = shipmentPK;
							jobHeaderCartage1.JH_A_JCL = jobClosedDate;
							forwardingTestDataCreator.CreateCartageData(jobHeaderCartage1.PK, out _, config.IsForwardingDataCancelled);
							factory.Save();

							accountingTestDataCreator.CreateAccountingData(jobHeaderCartage1.PK, postDate, config.IsHotChequeCancelled, config.IsHotChequeLinkedToAH);
							jobHeaderCartage1.JH_Status = JobHeaderStatus.Closed.Code;
							factory.Save();
						}
					}
				}
			}

			var portMessagingArchiveTestDataCreator = ObjectFactory.Get<IPortMessagingTestDataCreator>();
			foreach (var shipmentPK in config.ShipmentPKs)
			{
				portMessagingArchiveTestDataCreator.CreatePackLineData(shipmentPK);
			}

			factory.Save();

			if (config.NeedTableRecordCounts)
			{
				config.TableRecordCountsWithTestRecords = TableRecordCountsHelper.GetTableRecordCounts();
			}
		}

		public static ICustomsAttachedDeclarationDataCreator GetCustomsAttachedDeclarationDataCreator(string countryCode)
		{
			var providers = ObjectFactory.Get<Hashtable>("CustomsAttachedDeclarationDataProvider");
			var providerHandle = (ObjectHandle)providers[countryCode];
			return (ICustomsAttachedDeclarationDataCreator)providerHandle?.GetObject();
		}

		public static void ForExcludedRatingData(bool includeJobShipment)
		{
			var factory = new BusinessObjectFactory();
			var ratingTestDataCreator = ObjectFactory.Get<IRatingTestDataCreator>();
			var ratingHeaderPK = ratingTestDataCreator.CreateAttachedRatingData(1.ToString(), isCancelled: false);

			factory.Save();

			var jobHeader = factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.JH_Status = JobHeaderStatus.Closed.Code;
			jobHeader.JH_A_JCL = ZDateTime.BrettsBirthday;
			jobHeader.JH_JobNum = "T00001";

			var attachmentSet = factory.New<RateAttachmentSet>();
			attachmentSet.TS_AttachmentName = "Test Document";

			var rateAttachment = factory.New<RateAttachment>();
			rateAttachment.TA_TH = ratingHeaderPK;
			rateAttachment.TA_TS = attachmentSet.PK;

			factory.Save();

			if (includeJobShipment)
			{
				var jobShipment = factory.New<ForwardingShipment>();
				jobShipment.JS_UniqueConsignRef = "S00001";
				jobShipment.JS_TH_OneTimeQuote = ratingHeaderPK;

				jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				jobHeader.JH_ParentID = jobShipment.PK;
			}
			else
			{
				jobHeader.JH_ParentTableCode = RatingHeaderSchema.Constants.Prefix;
				jobHeader.JH_ParentID = ratingHeaderPK;
			}

			factory.Save();
		}

		public static void ForSkippedJobHeader(TestConfiguration config)
		{
			var factory = new BusinessObjectFactory();

			var accountingTestDataCreator = ObjectFactory.Get<IAccountingTestDataCreator>();
			var forwardingTestDataCreator = ObjectFactory.Get<IForwardingTestDataCreator>();
			accountingTestDataCreator.CreatePeriods(200701, new ZDateTime(2007, 1, 1), new ZDateTime(2007, 1, 31), config.IsPeriodClosed);
			factory.Save();

			var oldDate = ZDateTime.Today.AddYears(-1);
			var postDate = new ZDateTime(2007, 1, 14);
			forwardingTestDataCreator.CreateShipmentData(out var x);
			var companyFilter1 = new ZQuery(GlbCompanySchema.GC_Code, "EDI");
			_ = companyFilter1.AddToFilter(new ZQuery(GlbCompanySchema.GC_IsActive, true));
			var companiesByOrgProxy1 = factory.Load<GlbCompany>(companyFilter1);
			var jobHeaderShipment1WorkingStatus = factory.NewJobForTesting<JobHeader>();
			jobHeaderShipment1WorkingStatus.JH_GC = companiesByOrgProxy1[0].PK;
			jobHeaderShipment1WorkingStatus.JH_JobNum = "7777";
			jobHeaderShipment1WorkingStatus.JH_SystemLastEditTimeUtc = oldDate;
			jobHeaderShipment1WorkingStatus.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeaderShipment1WorkingStatus.JH_ParentID = x;
			factory.Save();

			accountingTestDataCreator.CreateAccountingData(jobHeaderShipment1WorkingStatus.PK, postDate, config.IsHotChequeCancelled, config.IsHotChequeLinkedToAH);
			jobHeaderShipment1WorkingStatus.JH_Status = JobHeaderStatus.Closed.Code;
			factory.Save();

			var companyFilter = new ZQuery(GlbCompanySchema.GC_Code, "SIN");
			_ = companyFilter.AddToFilter(new ZQuery(GlbCompanySchema.GC_IsActive, true));
			var companiesByOrgProxy = factory.Load<GlbCompany>(companyFilter);
			var jobHeaderShipment2WorkingStatus = factory.NewJobForTesting<JobHeader>();
			jobHeaderShipment2WorkingStatus.JH_GC = companiesByOrgProxy[0].PK;
			jobHeaderShipment2WorkingStatus.JH_JobNum = jobHeaderShipment1WorkingStatus.JH_JobNum;
			jobHeaderShipment2WorkingStatus.JH_SystemLastEditTimeUtc = oldDate;
			jobHeaderShipment2WorkingStatus.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			jobHeaderShipment2WorkingStatus.JH_ParentID = x;
			factory.Save();

			accountingTestDataCreator.CreateAccountingData(jobHeaderShipment2WorkingStatus.PK, postDate, config.IsHotChequeCancelled, config.IsHotChequeLinkedToAH);
			jobHeaderShipment2WorkingStatus.JH_Status = JobHeaderStatus.Working.Code;
			factory.Save();
		}
	}
}
