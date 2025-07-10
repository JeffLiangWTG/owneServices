using System;
using Enterprise.Customs.GB.CDS;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.Messaging.Business.EDIMessage;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.GB.Module.CDSDIS.Testing
{
	class CDSDISQueryTests
	{
		[TestedType(typeof(CDSDISQueryFilterStripBusinessObject))]
		class CDSDISQueryFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
		{
			protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new CDSDISQueryFilterStripBusinessObject();

			public void TestMessageFilter()
			{
				var msg1 = Factory.New<CDSDISQueryMessage>();
				var msg2 = Factory.New<CDSEDIMessage>();
				var msg3 = Factory.New<CDSDISQueryMessage>();

				msg3.EM_ReceiveTransmit = Direction.Receive;

				Factory.Save();

				var filterStripBO = GetNewFilterStripBusinessObject();

				var items = Factory.Load(typeof(CDSEDIMessage), filterStripBO.Filter);
				AssertEquals(1, items.Length);
				AssertEquals(msg1.PK, items[0].PK);
			}

			public void TestGetModuleFiltersCore()
			{
				var stripBO = GetNewFilterStripBusinessObject();
				var refFilter = (ModuleTextFilter)stripBO["Parameters/Reference"];
				AssertNotNull(refFilter);
				var statusFilter = (ModuleTextFilter)stripBO["Status"];
				AssertNotNull(statusFilter);
				var ownerFilter = (ModuleTextFilter)stripBO["Owner"];
				AssertNotNull(ownerFilter);
			}
		}

		[TestedType(typeof(CDSDISQueryModule))]
		class CDSDISQueryModuleBasherTest : ZModuleBasherTest
		{
			protected override ModuleIdentifier GetModuleID()
			{
				return ModuleIDs.Customs.EU.GB.CDSDISQuery;
			}

			protected override string CountryCode => Constants.CountryCodes.UnitedKingdom;
		}

		[TestedType(typeof(CDSDISQueryController))]
		class CDSDISQueryControllerBasherTest : ZControllerBasherTest
		{
			public override Type ControllerToBashType
			{
				get { return typeof(CDSDISQueryController); }
			}

			protected override string CountryCode
			{
				get { return Constants.CountryCodes.UnitedKingdom; }
			}

			protected override Type GetBusinessObjectType()
			{
				return typeof(CDSDISQueryMessage);
			}

			protected override ControllerID GetControllerID()
			{
				return ControllerIDs.Customs.GB.CDSDISQueryController;
			}
		}
	}
}
