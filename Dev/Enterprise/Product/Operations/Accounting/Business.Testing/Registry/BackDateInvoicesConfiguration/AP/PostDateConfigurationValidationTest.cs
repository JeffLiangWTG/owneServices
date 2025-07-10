using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	public abstract class PostDateConfigurationValidationTest : TestCaseWithFactory
	{
		public void TestValidateJobType()
		{
			AssertNoErrors("Precondition: JobType should not have errors.", BizObj.JobTypeInfo);

			BizObj.JobType = "";
			AssertHasErrors(BizObj.JobTypeInfo);

			BizObj.JobType = "ABC";
			AssertHasErrors(BizObj.JobTypeInfo);

			BizObj.JobType = "CLL";
			AssertNoErrors(BizObj.JobTypeInfo);

			PostDateConfiguration setting1 = BizObjCollection.AddNew();
			PostDateConfiguration setting3 = BizObjCollection.AddNew();

			AssertNoErrors("Precondition: setting1.JobTypeInfo should not have errors.", setting1.JobTypeInfo);
			AssertNoErrors("Precondition: setting3.JobTypeInfo should not have errors.", setting3.JobTypeInfo);

			setting1.JobType = PostDateConfigurationLookups.JobTypeAdditionalCodes.All;
			setting1.SignificantDateCode = PostDateConfigurationLookups.SignificantDateCodes.ActualArrivalDate;
			setting3.JobType = PostDateConfigurationLookups.JobTypeAdditionalCodes.All;
			setting3.SignificantDateCode = PostDateConfigurationLookups.SignificantDateCodes.DeliveryDate;

			string expectedError = "At least one more record already sets revenue recognition behaviour for the same Job parameters.";
			BizObjCollection.RunPreSaveValidation();
			AssertHasErrors(expectedError, setting1.JobTypeInfo);
			AssertHasErrors(expectedError, setting3.JobTypeInfo);

			setting1.JobType = "AWB";
			BizObjCollection.RunPreSaveValidation();
			AssertHasErrors(expectedError, setting1.JobTypeInfo);
			AssertHasErrors(setting3.JobTypeInfo);

			setting3.JobType = "SHP";
			setting3.DirectionCode = Constants.FreightShipmentDirection.Code.Export;
			setting3.Mode = Enterprise.Core.Constants.TransportModes.Sea;
			setting3.BrokerCode = PostDateConfigurationLookups.BrokerCodes.External;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertNoErrors(setting3.JobTypeInfo);

			setting3.Mode = Enterprise.Core.Constants.TransportModes.Courier;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertNoErrors(setting3.JobTypeInfo);

			setting3.Mode = PostDateConfigurationLookups.ModeAdditionalCodes.All;
			BizObjCollection.RunPreSaveValidation();
			AssertNoErrors(setting1.JobTypeInfo);
			AssertNoErrors(setting3.JobTypeInfo);
		}

		public void TestValidateDirection()
		{
			BizObj.JobType = "ABC";

			AssertNoErrors("Precondition: Direction should not have errors.", BizObj.DirectionCodeInfo);

			BizObj.DirectionCode = "ABC";
			AssertHasErrors(BizObj.DirectionCodeInfo);

			BizObj.DirectionCode = "";
			AssertHasErrors(BizObj.DirectionCodeInfo);

			BizObj.JobType = "AWB";
			BizObj.DirectionCode = "";
			AssertNoErrors(BizObj.DirectionCodeInfo);

			BizObj.DirectionCode = Constants.FreightShipmentDirection.Code.Import;
			AssertNoErrors(BizObj.DirectionCodeInfo);
		}

		public void TestValidateMode()
		{
			BizObj.JobType = "ABC";

			string expectedError = "Only 'Air', 'Sea', 'Road', 'Rail' and 'All' values are relevant for this Job Type.";
			AssertNoErrors("Precondition: Mode should not have errors.", BizObj.ModeInfo);

			BizObj.Mode = "ABC";
			AssertHasErrors(BizObj.ModeInfo);

			BizObj.Mode = "";
			AssertHasErrors(BizObj.ModeInfo);

			BizObj.JobType = "AWB";
			BizObj.Mode = "";
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.Mode = Enterprise.Core.Constants.TransportModes.Courier;
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.JobType = "CLL";
			BizObj.Mode = BizObj.Mode;
			AssertHasError(BizObj.ModeInfo, expectedError);

			BizObj.Mode = Enterprise.Core.Constants.TransportModes.Air;
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.Mode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.Mode = Enterprise.Core.Constants.TransportModes.Road;
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.Mode = Enterprise.Core.Constants.TransportModes.Rail;
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.Mode = PostDateConfigurationLookups.ModeAdditionalCodes.All;
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.Mode = Enterprise.Core.Constants.TransportModes.SeaAir;
			AssertHasError(BizObj.ModeInfo, expectedError);

			BizObj.JobType = "CSH";
			BizObj.Mode = Enterprise.Core.Constants.TransportModes.AirSea;
			AssertHasError(BizObj.ModeInfo, expectedError);

			BizObj.Mode = Enterprise.Core.Constants.TransportModes.Air;
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.Mode = Enterprise.Core.Constants.TransportModes.Sea;
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.Mode = Enterprise.Core.Constants.TransportModes.Road;
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.Mode = Enterprise.Core.Constants.TransportModes.Rail;
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.Mode = PostDateConfigurationLookups.ModeAdditionalCodes.All;
			AssertNoErrors(BizObj.ModeInfo);

			BizObj.Mode = Enterprise.Core.Constants.TransportModes.SeaAir;
			AssertHasError(BizObj.ModeInfo, expectedError);

			BizObj.JobType = "SHP";
			BizObj.Mode = BizObj.Mode;
			AssertNoErrors(BizObj.ModeInfo);
		}

		public void TestValidateRecognitionDateOption()
		{
			AssertNoErrors("Precondition: Mode should not have errors.", BizObj.SignificantDateCodeInfo);

			BizObj.SignificantDateCode = "ABC";
			AssertHasErrors(BizObj.SignificantDateCodeInfo);

			BizObj.SignificantDateCode = "";
			AssertHasErrors(BizObj.SignificantDateCodeInfo);

			BizObj.SignificantDateCode = PostDateConfigurationLookups.SignificantDateCodes.InvoiceAddDate;
			AssertNoErrors(BizObj.SignificantDateCodeInfo);

			// Permitted date options for various job types

			AssertIsPermittedRecognitionDateOption("", CompleteSignificantDateList);
			AssertIsPermittedRecognitionDateOption("ALL", SignificantDatePermittedForAllList);
			AssertIsPermittedRecognitionDateOption("SHP", SignificantDatePermittedForShipmentsList);
			AssertIsPermittedRecognitionDateOption("BRK", SignificantDatePermittedForDeclarationsList);
			foreach (ICodeDescription jobType in JobTypeList)
			{
				if (jobType.Code != "ALL" && jobType.Code != "SHP" && jobType.Code != "FCN" && jobType.Code != "GCN" && jobType.Code != "BRK")
				{
					AssertIsPermittedRecognitionDateOption(jobType.Code, SignificantDatePermittedForOthersList);
				}
			}
		}

		public void TestValidateBroker()
		{
			BizObj.JobType = "ABC";

			AssertNoErrors("Precondition: Broker should not have errors.", BizObj.BrokerCodeInfo);

			BizObj.BrokerCode = "ABC";
			AssertHasErrors(BizObj.BrokerCodeInfo);

			BizObj.BrokerCode = "";
			AssertHasErrors(BizObj.BrokerCodeInfo);

			BizObj.BrokerCode = PostDateConfigurationLookups.BrokerCodes.All;
			AssertNoErrors(BizObj.BrokerCodeInfo);

			BizObj.BrokerCode = PostDateConfigurationLookups.BrokerCodes.Internal;
			AssertNoErrors(BizObj.BrokerCodeInfo);

			BizObj.BrokerCode = PostDateConfigurationLookups.BrokerCodes.External;
			AssertNoErrors(BizObj.BrokerCodeInfo);
		}

		public void TestValidateReversalRule()
		{
			AssertNoErrors("Precondition: Reversal Rule should not have errors.", BizObj.ReversalRuleInfo);

			BizObj.ReversalRule = "ABC";
			AssertHasErrors(BizObj.ReversalRuleInfo);

			BizObj.ReversalRule = "";
			AssertHasErrors(BizObj.ReversalRuleInfo);
		}

		public abstract void TestRunPreSaveValidation();

		#region Implementation

		protected abstract PostDateConfiguration GetNewBizObj { get; }

		protected abstract PostDateConfigurationCollection GetNewBizObjCollection { get; }

		protected override void SetUp()
		{
			base.SetUp();
			BizObj = GetNewBizObj;
			BizObjCollection = GetNewBizObjCollection;
		}

		protected PostDateConfiguration BizObj;
		protected PostDateConfigurationCollection BizObjCollection;

		CodeDescriptionPairList completeSignificantDateList;
		CodeDescriptionPairList CompleteSignificantDateList
		{
			get { return completeSignificantDateList ?? (completeSignificantDateList = PostDateConfigurationLookups.CompleteSignificantDateList); }
		}

		void AssertIsPermittedRecognitionDateOption(string jobType, CodeDescriptionPairList permittedOptionsList)
		{
			BizObj.JobType = jobType;
			foreach (ICodeDescription availableRecognitionDateOption in CompleteSignificantDateList)
			{
				BizObj.SignificantDateCode = availableRecognitionDateOption.Code;
				Assert(String.Format("Recognition Date Option '{0}' is not permitted for job type {1}", availableRecognitionDateOption.Code, jobType),
					permittedOptionsList.Contains(availableRecognitionDateOption) ^ BizObj.SignificantDateCodeInfo.HasErrors());
			}
		}

		CodeDescriptionPairList fSignificantDatePermittedForAllList;
		CodeDescriptionPairList SignificantDatePermittedForAllList
		{
			get { return fSignificantDatePermittedForAllList ?? (fSignificantDatePermittedForAllList = BizObj.PostDateConfigurationLookups.SignificantDatePermittedForAllList); }
		}

		CodeDescriptionPairList fSignificantDatePermittedForOthersList;
		CodeDescriptionPairList SignificantDatePermittedForOthersList
		{
			get { return fSignificantDatePermittedForOthersList ?? (fSignificantDatePermittedForOthersList = BizObj.PostDateConfigurationLookups.SignificantDatePermittedForOthersList); }
		}

		CodeDescriptionPairList fSignificantDatePermittedForShipmentsList;
		CodeDescriptionPairList SignificantDatePermittedForShipmentsList
		{
			get { return fSignificantDatePermittedForShipmentsList ?? (fSignificantDatePermittedForShipmentsList = BizObj.PostDateConfigurationLookups.SignificantDatePermittedForShipmentsList); }
		}

		CodeDescriptionPairList fSignificantDatePermittedForDeclarationsList;
		CodeDescriptionPairList SignificantDatePermittedForDeclarationsList
		{
			get { return fSignificantDatePermittedForDeclarationsList ?? (fSignificantDatePermittedForDeclarationsList = BizObj.PostDateConfigurationLookups.SignificantDatePermittedForDeclarationsList); }
		}

		CodeDescriptionPairList fJobTypeList;
		CodeDescriptionPairList JobTypeList
		{
			get { return fJobTypeList ?? (fJobTypeList = BizObj.JobTypeList); }
		}

		#endregion
	}
}