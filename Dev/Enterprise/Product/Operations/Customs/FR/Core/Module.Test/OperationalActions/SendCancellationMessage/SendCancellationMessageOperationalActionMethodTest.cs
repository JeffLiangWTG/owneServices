using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.Module;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.OperationalActions.Testing
{
	[TestedType(typeof(SendCancellationMessageOperationalActionMethod))]
	public class SendCancellationMessageOperationalActionMethodTest : OperationalActionMethodTest<SendCancellationMessageOperationalActionMethod>
	{
		protected override SendCancellationMessageOperationalActionMethod NewMethod()
		{
			return new SendCancellationMessageOperationalActionMethod();
		}

		public void TestGetFilterRequirements()
		{
			var filterRequirements = Method.GetFilterRequirements();

			AssertContainsExactElementsInAnyOrder(
				"Method should meet constraints of french jurisdiction and Delta I/E enabled for imports or exports.",
				new List<string>
				{
					$"{new FilterIsUnderFrenchCustomsJurisdictionConstraint().Name}:Y",
					$"{new FilterIsDeltaIEEnabledForImportsOrExportsConstraint().Name}:Y"
				},
				filterRequirements.Select(s => $"{s.ConstraintName}:{string.Join(",", s.Select(v => v))}"));
		}

		public void TestRunWithoutUITrue()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interface;
			var entryHeader1_1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_1.CorrelationID = "1";
			entryHeader1_1.MRN = "1";
			entryHeader1_1.CRN = "1";
			var entryHeader1_2 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_2.CorrelationID = "2";
			entryHeader1_2.MRN = "2";
			entryHeader1_2.CRN = "2";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryHeader2_1 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2_1.CorrelationID = "3";
			entryHeader2_1.MRN = "3";
			entryHeader2_1.CRN = "3";
			Factory.Save();

			var applicator = Method.NewApplicator(Factory, null);
			applicator.Build(new ZGuid[] { entryHeader1_1.PK, entryHeader1_2.PK, entryHeader2_1.PK });

			AssertEquals("One target is invalid, method should not run with UI", true, Method.RunWithoutUI);
			AssertExceptionThrown<NotSupportedException>("When a target is invalid and no UI should be opened then an exception must be thrown", new AnonymousMethod(() => Method.NewGuiControl()));
		}

		public void TestRunWithoutUIFalse()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryHeader1_1 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_1.CorrelationID = "1";
			entryHeader1_1.MRN = "1";
			entryHeader1_1.CRN = "1";
			var entryHeader1_2 = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader1_2.CorrelationID = "2";
			entryHeader1_2.MRN = "2";
			entryHeader1_2.CRN = "2";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			var entryHeader2_1 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader2_1.CorrelationID = "3";
			entryHeader2_1.MRN = "3";
			entryHeader2_1.CRN = "3";
			Factory.Save();

			var applicator = Method.NewApplicator(Factory, null);
			applicator.Build(new ZGuid[] { entryHeader1_1.PK, entryHeader1_2.PK, entryHeader2_1.PK });

			AssertEquals("All targets are valid, method should run with UI", false, Method.RunWithoutUI);
			AssertEquals("All targets are valid, method should run with UI", true, Method.HasControl);
		}
	}
}
