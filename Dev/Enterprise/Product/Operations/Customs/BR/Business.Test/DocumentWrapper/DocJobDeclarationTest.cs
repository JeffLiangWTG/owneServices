using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(DocDeclaration))]
	sealed class DocJobDeclarationTest : DocBaseJobDeclarationAbstractTest<JobDeclaration, DocDeclaration>
	{
		public void TestDocCusEntryHeaderCollectionType()
		{
			AssertEquals(typeof(DocCusEntryHeaderCollection), DeclarationWrapper.RateEntryHeaders.GetType());
		}

		public override void TestTransportModeDescription()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("TransportModeDescription", "Air", DeclarationWrapper.TransportModeDescription);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("TransportModeDescription", "Sea", DeclarationWrapper.TransportModeDescription);
		}

		protected override JobDeclaration GetNewJobDeclaration()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			return (JobDeclaration)declaration;
		}

		#region Implementation

		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes.Brazil; }
		}

		protected override DocDeclaration CreateDeclarationWrapper(JobDeclaration declaration)
		{
			var result = DocDeclaration.New(declaration, Factory);
			((IBODocDataProvider)result).SetDocWrapperContext(new Dictionary<string, object>());
			result.SetReportNameForTesting("Report Name");
			return result;
		}

		#endregion
	}
}
