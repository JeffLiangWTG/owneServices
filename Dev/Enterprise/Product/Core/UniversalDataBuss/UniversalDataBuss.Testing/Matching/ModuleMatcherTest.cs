using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Types;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Matching;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.ZArchitecture.Schema;
using DummyBizo = Enterprise.ZArchitecture.Business.Testing.DummyBizOWithAutoLogs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.UniversalDataBuss.Matching.Testing
{
	class ModuleMatcherTest : TestCaseWithUniversalObjectFactory
	{
		public void TestShortReferencesAreIgnored()
		{
			var genericOrg = OrgCreator.CreateBusinessObject("GENERIC", "ENTERPRISES", "2345");

			var sendingFwdrDataObject = OrgCreator.CreateDataObject("GENERIC", "ENTERPRISES", "2345");
			sendingFwdrDataObject.AddressType = nameof(MatchableOrganizationType.SendingForwarderAddress);

			var departureCFSDataObject = OrgCreator.CreateDataObject("GENERIC", "ENTERPRISES", "2345");
			departureCFSDataObject.AddressType = nameof(MatchableOrganizationType.DepartureCFSAddress);

			var consignorDataObject = OrgCreator.CreateDataObject("GENERIC", "ENTERPRISES", "2345");
			consignorDataObject.AddressType = nameof(MatchableOrganizationType.ConsignorDocumentaryAddress);

			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.AgentsReference = "BIGGER";
			dataObject.CFSReference = "SHORT";
			dataObject.OwnerRef = "TINY";
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>()
			{
				sendingFwdrDataObject,
				departureCFSDataObject,
				consignorDataObject,
			});

			Factory.SaveForTesting();

			eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var matcher = new ModuleMatcher<IShipmentDataObjectReader, DummyBizo>(Factory);
			matcher.AddPossibleMatchReferenceAndOrgHeader(ReferenceElementName.AgentsReference, DummyBizoSchema.Z0_Description, MatchableOrganizationType.SendingForwarderAddress, DummyBizoSchema.Z0_Guid, new Score() { FullMatch = 90, Conflict = -90, ReferenceOnlyMatch = 10 });
			matcher.AddPossibleMatchReferenceAndOrgHeader(ReferenceElementName.CFSReference, DummyBizoSchema.Z0_VarCharMax, MatchableOrganizationType.DepartureCFSAddress, DummyBizoSchema.Z0_Guid, new Score() { FullMatch = 90, Conflict = -90, ReferenceOnlyMatch = 10 });
			matcher.AddPossibleMatchReferenceAndOrgHeader(ReferenceElementName.OwnerRef, DummyBizoSchema.Z0_NVarChar, MatchableOrganizationType.ConsignorDocumentaryAddress, DummyBizoSchema.Z0_Guid, new Score() { FullMatch = 90, Conflict = -90, ReferenceOnlyMatch = 10 });

			var matchResult = matcher.GetBestMatch(new DummyReader(dataObject));

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("matchResult.MatchingLog", @"
Starting Reference/Party ID scoring.
Have incoming values for AgentsReference.
Found no potential matches using any of the incoming values.
No Reference/Party ID matches were found in this module.
".Trim(), string.Join("\r\n", matchResult.GetMatchingLogsForTesting()));

				AssertEquals("matchResult.Success", false, matchResult.Success);
				AssertEquals("matchResult.MatchFound", null, matchResult.MatchFound);
			});
		}

		public void TestMisMatchWorks()
		{
			var genericOrg = OrgCreator.CreateBusinessObject("GENERIC", "ENTERPRISES", "2345");

			var sendingFwdrDataObject = OrgCreator.CreateDataObject("GENERIC", "ENTERPRISES", "2345");
			sendingFwdrDataObject.AddressType = nameof(MatchableOrganizationType.SendingForwarderAddress);

			var departureCFSDataObject = OrgCreator.CreateDataObject("GENERIC", "ENTERPRISES", "2345");
			departureCFSDataObject.AddressType = nameof(MatchableOrganizationType.DepartureCFSAddress);

			var dummyBOMatch = Factory.New<DummyBizo>();
			dummyBOMatch.HumanReadableNameForTest = "Dummy BO X00001234";
			dummyBOMatch.Z0_Description = "MATCH_D-123456";
			dummyBOMatch.Z0_VarCharMax = "MATCH_V-123456";
			dummyBOMatch.Z0_NVarChar = "MATCH_N-123456";

			dummyBOMatch.Z0_Guid = genericOrg.PK;

			Factory.SaveForTesting(); // May be able to remove.

			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.AgentsReference = "MATCH_D-123456";
			dataObject.CFSReference = "NON-MATCH_V-123456";
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>()
			{
				sendingFwdrDataObject,
				departureCFSDataObject,
			});

			eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var matcher = new ModuleMatcher<IShipmentDataObjectReader, DummyBizo>(Factory);
			matcher.AddPossibleMatchReferenceAndOrgHeader(ReferenceElementName.AgentsReference, DummyBizoSchema.Z0_Description, MatchableOrganizationType.SendingForwarderAddress, DummyBizoSchema.Z0_Guid, new Score() { FullMatch = 90, Conflict = -90, ReferenceOnlyMatch = 10 });
			matcher.AddPossibleMatchReferenceAndOrgHeader(ReferenceElementName.CFSReference, DummyBizoSchema.Z0_VarCharMax, MatchableOrganizationType.DepartureCFSAddress, DummyBizoSchema.Z0_Guid, new Score() { FullMatch = 90, Conflict = -90, ReferenceOnlyMatch = 10 });
			matcher.AddPossibleMatchReferenceAndOrgHeader(ReferenceElementName.OwnerRef, DummyBizoSchema.Z0_NVarChar, MatchableOrganizationType.ConsignorDocumentaryAddress, DummyBizoSchema.Z0_Guid, new Score() { FullMatch = 90, Conflict = -90, ReferenceOnlyMatch = 10 });

			var matchResult = matcher.GetBestMatch(new DummyReader(dataObject));

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("matchResult.MatchingLog", @"
Starting Reference/Party ID scoring.
Have incoming values for AgentsReference, CFSReference.
Found 1 potential match...
Scoring Dummy BO X00001234:-
Match on AgentsReference and SendingForwarderAddress, 90 points.
Conflict on CFSReference, -90 points.
Final Score: 0 points.
Best Reference/Party ID match is Dummy BO X00001234 with a score of 0, but the minimum match score is 90. Match failed.
".Trim(), string.Join("\r\n", matchResult.GetMatchingLogsForTesting()));

				AssertEquals("matchResult.Success", false, matchResult.Success);
				AssertEquals("matchResult.MatchFound", null, matchResult.MatchFound);
			});
		}

		public void TestPartialMatchWorks()
		{
			var genericOrg = OrgCreator.CreateBusinessObject("GENERIC", "ENTERPRISES", "2345");

			var sendingFwdrDataObject = OrgCreator.CreateDataObject("GENERIC", "ENTERPRISES", "2345");
			sendingFwdrDataObject.AddressType = nameof(MatchableOrganizationType.SendingForwarderAddress);

			var departureCFSDataObject = OrgCreator.CreateDataObject("NON", "MATCH", "4567");
			departureCFSDataObject.AddressType = nameof(MatchableOrganizationType.DepartureCFSAddress);

			var dummyBOMatch = Factory.New<DummyBizo>();
			dummyBOMatch.HumanReadableNameForTest = "Dummy BO X00001234";
			dummyBOMatch.Z0_Description = "MATCH_D-123456";
			dummyBOMatch.Z0_VarCharMax = "MATCH_V-123456";
			dummyBOMatch.Z0_NVarChar = "MATCH_N-123456";

			dummyBOMatch.Z0_Guid = genericOrg.PK;

			Factory.SaveForTesting(); // May be able to remove.

			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.AgentsReference = "MATCH_D-123456";
			dataObject.CFSReference = "MATCH_V-123456";
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>()
			{
				sendingFwdrDataObject,
				departureCFSDataObject,
			});

			eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var matcher = new ModuleMatcher<IShipmentDataObjectReader, DummyBizo>(Factory);
			matcher.AddPossibleMatchReferenceAndOrgHeader(ReferenceElementName.AgentsReference, DummyBizoSchema.Z0_Description, MatchableOrganizationType.SendingForwarderAddress, DummyBizoSchema.Z0_Guid, new Score() { FullMatch = 90, Conflict = -90, ReferenceOnlyMatch = 10 });
			matcher.AddPossibleMatchReferenceAndOrgHeader(ReferenceElementName.CFSReference, DummyBizoSchema.Z0_VarCharMax, MatchableOrganizationType.DepartureCFSAddress, DummyBizoSchema.Z0_Guid, new Score() { FullMatch = 90, Conflict = -90, ReferenceOnlyMatch = 10 });
			matcher.AddPossibleMatchReferenceAndOrgHeader(ReferenceElementName.OwnerRef, DummyBizoSchema.Z0_NVarChar, MatchableOrganizationType.ConsignorDocumentaryAddress, DummyBizoSchema.Z0_Guid, new Score() { FullMatch = 90, Conflict = -90, ReferenceOnlyMatch = 10 });

			var matchResult = matcher.GetBestMatch(new DummyReader(dataObject));

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("matchResult.MatchingLog", @"
Starting Reference/Party ID scoring.
Have incoming values for AgentsReference, CFSReference.
Found 1 potential match...
Scoring Dummy BO X00001234:-
Match on AgentsReference and SendingForwarderAddress, 90 points.
Partial Match on on CFSReference without DepartureCFSAddress, 10 points.
Final Score: 140 points.
Matched to Dummy BO X00001234 with a Reference/Party ID match score of 140.
".Trim(), string.Join("\r\n", matchResult.GetMatchingLogsForTesting()));

				AssertEquals("matchResult.Success", true, matchResult.Success);
				AssertEquals("matchResult.MatchFound", dummyBOMatch, matchResult.MatchFound);
			});
		}

		public void TestCombinationGivesExponentialScore()
		{
			var genericOrg = OrgCreator.CreateBusinessObject("GENERIC", "ENTERPRISES", "2345");

			var sendingFwdrDataObject = OrgCreator.CreateDataObject("GENERIC", "ENTERPRISES", "2345");
			sendingFwdrDataObject.AddressType = nameof(MatchableOrganizationType.SendingForwarderAddress);

			var departureCFSDataObject = OrgCreator.CreateDataObject("NON", "MATCH", "4567");
			departureCFSDataObject.AddressType = nameof(MatchableOrganizationType.DepartureCFSAddress);

			var consignorDataObject = OrgCreator.CreateDataObject("GENERIC", "ENTERPRISES", "2345");
			consignorDataObject.AddressType = nameof(MatchableOrganizationType.ConsignorDocumentaryAddress);

			var dummyBOMatch = Factory.New<DummyBizo>();
			dummyBOMatch.HumanReadableNameForTest = "Dummy BO X00001234";
			dummyBOMatch.Z0_Description = "MATCH_D-123456";
			dummyBOMatch.Z0_VarCharMax = "MATCH_V-123456";
			dummyBOMatch.Z0_NVarChar = "MATCH_N-123456";

			dummyBOMatch.Z0_Guid = genericOrg.PK;

			Factory.SaveForTesting(); // May be able to remove.

			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.AgentsReference = "MATCH_D-123456";
			dataObject.OwnerRef = "MATCH_N-123456";
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>()
			{
				sendingFwdrDataObject,
				departureCFSDataObject,
				consignorDataObject,
			});

			eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var matcher = new ModuleMatcher<IShipmentDataObjectReader, DummyBizo>(Factory);
			matcher.AddPossibleMatchReferenceAndOrgHeader(ReferenceElementName.AgentsReference, DummyBizoSchema.Z0_Description, MatchableOrganizationType.SendingForwarderAddress, DummyBizoSchema.Z0_Guid, new Score() { FullMatch = 90, Conflict = -90, ReferenceOnlyMatch = 10 });
			matcher.AddPossibleMatchReferenceAndOrgHeader(ReferenceElementName.CFSReference, DummyBizoSchema.Z0_VarCharMax, MatchableOrganizationType.DepartureCFSAddress, DummyBizoSchema.Z0_Guid, new Score() { FullMatch = 90, Conflict = -90, ReferenceOnlyMatch = 10 });
			matcher.AddPossibleMatchReferenceAndOrgHeader(ReferenceElementName.OwnerRef, DummyBizoSchema.Z0_NVarChar, MatchableOrganizationType.ConsignorDocumentaryAddress, DummyBizoSchema.Z0_Guid, new Score() { FullMatch = 90, Conflict = -90, ReferenceOnlyMatch = 10 });

			var matchResult = matcher.GetBestMatch(new DummyReader(dataObject));

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("matchResult.MatchingLog", @"
Starting Reference/Party ID scoring.
Have incoming values for AgentsReference, OwnerRef.
Found 1 potential match...
Scoring Dummy BO X00001234:-
Match on AgentsReference and SendingForwarderAddress, 90 points.
Match on OwnerRef and ConsignorDocumentaryAddress, 90 points.
Final Score: 220 points.
Matched to Dummy BO X00001234 with a Reference/Party ID match score of 220.
".Trim(), string.Join("\r\n", matchResult.GetMatchingLogsForTesting()));

				AssertEquals("matchResult.Success", true, matchResult.Success);
				AssertEquals("matchResult.MatchFound", dummyBOMatch, matchResult.MatchFound);
			});
		}

		public void TestFallbackToMostRecentlyCreated()
		{
			var genericOrg = OrgCreator.CreateBusinessObject("GENERIC", "ENTERPRISES", "2345");

			var sendingFwdrDataObject = OrgCreator.CreateDataObject("GENERIC", "ENTERPRISES", "2345");
			sendingFwdrDataObject.AddressType = nameof(MatchableOrganizationType.SendingForwarderAddress);

			var departureCFSDataObject = OrgCreator.CreateDataObject("GENERIC", "ENTERPRISES", "2345");
			departureCFSDataObject.AddressType = nameof(MatchableOrganizationType.DepartureCFSAddress);

			var consignorDataObject = OrgCreator.CreateDataObject("GENERIC", "ENTERPRISES", "2345");
			consignorDataObject.AddressType = nameof(MatchableOrganizationType.ConsignorDocumentaryAddress);

			var dummyBOMatch1 = Factory.New<DummyBizo>();
			dummyBOMatch1.HumanReadableNameForTest = "Dummy BO X00001111";
			dummyBOMatch1.Z0_Description = "MATCH_D-123456";
			dummyBOMatch1.Z0_VarCharMax = "MATCH_V-123456";
			dummyBOMatch1.Z0_Guid = genericOrg.PK;

			Factory.SaveForTesting();

			Thread.Sleep(1000);

			var dummyBOMatch2 = Factory.New<DummyBizo>();
			dummyBOMatch2.HumanReadableNameForTest = "Dummy BO X00002222";
			dummyBOMatch2.Z0_Description = "MATCH_D-123456";
			dummyBOMatch2.Z0_NVarChar = "MATCH_N-123456";
			dummyBOMatch2.Z0_Guid = genericOrg.PK;

			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.AgentsReference = "MATCH_D-123456";
			dataObject.CFSReference = "MATCH_V-123456";
			dataObject.OwnerRef = "MATCH_N-123456";
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>()
			{
				sendingFwdrDataObject,
				departureCFSDataObject,
				consignorDataObject,
			});

			Factory.SaveForTesting();

			eAdaptorRegistry.Instance.UniversalXMLEnableVerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var matcher = new ModuleMatcher<IShipmentDataObjectReader, DummyBizo>(Factory);
			matcher.AddPossibleMatchReferenceAndOrgHeader(ReferenceElementName.AgentsReference, DummyBizoSchema.Z0_Description, MatchableOrganizationType.SendingForwarderAddress, DummyBizoSchema.Z0_Guid, new Score() { FullMatch = 90, Conflict = -90, ReferenceOnlyMatch = 10 });
			matcher.AddPossibleMatchReferenceAndOrgHeader(ReferenceElementName.CFSReference, DummyBizoSchema.Z0_VarCharMax, MatchableOrganizationType.DepartureCFSAddress, DummyBizoSchema.Z0_Guid, new Score() { FullMatch = 90, Conflict = -90, ReferenceOnlyMatch = 10 });
			matcher.AddPossibleMatchReferenceAndOrgHeader(ReferenceElementName.OwnerRef, DummyBizoSchema.Z0_NVarChar, MatchableOrganizationType.ConsignorDocumentaryAddress, DummyBizoSchema.Z0_Guid, new Score() { FullMatch = 90, Conflict = -90, ReferenceOnlyMatch = 10 });

			var matchResult = matcher.GetBestMatch(new DummyReader(dataObject));

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("matchResult.MatchingLog", @"
Starting Reference/Party ID scoring.
Have incoming values for AgentsReference, CFSReference, OwnerRef.
Found 2 potential matches...
Scoring Dummy BO X00001111:-
Match on AgentsReference and SendingForwarderAddress, 90 points.
Match on CFSReference and DepartureCFSAddress, 90 points.
Final Score: 220 points.
Scoring Dummy BO X00002222:-
Match on AgentsReference and SendingForwarderAddress, 90 points.
Match on OwnerRef and ConsignorDocumentaryAddress, 90 points.
Final Score: 220 points.
2 matches with the same score, falling back to most recently created...
Matched to Dummy BO X00002222 with a Reference/Party ID match score of 220.
".Trim(), string.Join("\r\n", matchResult.GetMatchingLogsForTesting()));

				AssertEquals("matchResult.Success", true, matchResult.Success);
				AssertEquals("matchResult.MatchFound", dummyBOMatch2, matchResult.MatchFound);
			});
		}

		public void TestModuleMatcherBasicallyWorks()
		{
			var matchingOrgHeader = OrgCreator.CreateBusinessObject("BENNYS", "GUITAR", "7268");
			var matchingOrgDataObject = OrgCreator.CreateDataObject("BENNYS", "GUITAR", "7268");
			matchingOrgDataObject.AddressType = nameof(MatchableOrganizationType.DepartureCFSAddress);

			var nonMatchingOrgHeader = OrgCreator.CreateBusinessObject("TRANSITIONAL", "PROPOSITION", "3946");
			var nonMatchingOrgDataObject = OrgCreator.CreateDataObject("TRANSITIONAL", "PROPOSITION", "3946");
			nonMatchingOrgDataObject.AddressType = nameof(MatchableOrganizationType.ReceivingForwarderAddress);

			var dummyBONoMatch1 = Factory.New<DummyBizo>();
			dummyBONoMatch1.HumanReadableNameForTest = "Dummy BO X00001001 - WRONG!!!";
			dummyBONoMatch1.Z0_Description = "RER321789";
			dummyBONoMatch1.Z0_Guid = ZGuid.NewZGuid();

			var dummyBONoMatch2 = Factory.New<DummyBizo>();
			dummyBONoMatch2.HumanReadableNameForTest = "Dummy BO X00001002 - WRONG!!!";
			dummyBONoMatch2.Z0_Description = "FAF000123";
			dummyBONoMatch2.Z0_Guid = matchingOrgHeader.PK;

			var dummyBOMatch = Factory.New<DummyBizo>();
			dummyBOMatch.HumanReadableNameForTest = "Dummy BO X00001234";
			dummyBOMatch.Z0_Description = "RER321789";
			dummyBOMatch.Z0_Guid = matchingOrgHeader.PK;

			var dummyBONoMatch3 = Factory.New<DummyBizo>();
			dummyBONoMatch3.HumanReadableNameForTest = "Dummy BO X00001003 - WRONG!!!";
			dummyBONoMatch3.Z0_Description = "64U2DAI";
			dummyBONoMatch3.Z0_Guid = matchingOrgHeader.PK;

			var dummyBONoMatch4 = Factory.New<DummyBizo>();
			dummyBONoMatch4.HumanReadableNameForTest = "Dummy BO X00001004 - WRONG!!!";

			var dummyBONoMatch5 = Factory.New<DummyBizo>();
			dummyBONoMatch5.HumanReadableNameForTest = "Dummy BO X00001005 - WRONG!!!";
			dummyBONoMatch5.Z0_Description = "RER321789";
			dummyBONoMatch5.Z0_Guid = nonMatchingOrgHeader.PK;

			Factory.SaveForTesting(); // May be able to remove.

			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.BookingConfirmationReference = "RER321789";
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>()
			{
				matchingOrgDataObject,
				nonMatchingOrgDataObject,
			});

			var matcher = new ModuleMatcher<IShipmentDataObjectReader, DummyBizo>(Factory);
			matcher.AddPossibleMatchReferenceAndOrgHeader(ReferenceElementName.BookingConfirmationReference, DummyBizoSchema.Z0_Description, MatchableOrganizationType.DepartureCFSAddress, DummyBizoSchema.Z0_Guid, new Score() { FullMatch = 90 });

			var matchResult = matcher.GetBestMatch(new DummyReader(dataObject));

			CombineAssertions(delegate
			{
				AssertMultilineASCIIEquals("matchResult.MatchingLog"
					, "Matched to Dummy BO X00001234 with a Reference/Party ID match score of 90."
					, string.Join("\r\n", matchResult.GetMatchingLogsForTesting()));

				AssertEquals("matchResult.Success", true, matchResult.Success);
				AssertEquals("matchResult.MatchFound", dummyBOMatch, matchResult.MatchFound);
			});
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			this.OrgCreator = new OrganisationTestHelper(Factory);
		}

		OrganisationTestHelper OrgCreator;

		public class DummyReader : IShipmentDataObjectReader
		{
			public DummyReader(UniversalShipment shipment)
			{
				this.shipment = shipment;
			}
			readonly UniversalShipment shipment;

			public CargoWise.EntityFramework.BusinessObject GetExistingBusinessObject()
			{
				throw new NotImplementedException();
			}

			public IShipmentDataObjectReader ParentReader
			{
				get { return null; }
			}

			public ITopLevelDataObject DataObject
			{
				get { return shipment; }
			}

			public void ReadIntoBusinessObject(ref CargoWise.EntityFramework.BusinessObject targetBO)
			{
				throw new NotImplementedException();
			}

			public CargoWise.EntityFramework.BusinessObject ReadIntoTopLevelBusinessObject()
			{
				throw new NotImplementedException();
			}

			public IEnumerable<(string KeyValue, string KeySource)> ReadKeysForParallelism()
			{
				throw new NotImplementedException();
			}
		}

		#endregion

	}
}
