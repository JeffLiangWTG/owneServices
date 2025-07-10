using System;
using System.Linq;
using CargoWise.Application;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(VisualizerDocumentDataContextManager))]
	sealed class VisualizerDocumentDataContextManagerTest : ShipmentDataContextManagerTestCase<VisualizerDocumentDataContextManager, VisualizerDocumentData>
	{
		public new void TestAttributeIsOnBusinessObject()
		{
			var attribute = typeof(VisualizerDocumentData).GetAttribute<UniversalDataContextAttribute>();
			AssertNotNull("Must have UniversalDataContextAttribute applied.", attribute);
		}

		public void TestDataContextKey()
		{
			var data = GetNewBusinessObjectForTesting();

			IDataContextManager manager = new VisualizerDocumentDataContextManager();
			manager.Init(data);

			AssertEquals("DataContextKey", "C0000001", manager.DataContextKey);
		}

		public void TestDataContextType()
		{
			var data = GetNewBusinessObjectForTesting();

			IDataContextManager manager = new VisualizerDocumentDataContextManager();
			manager.Init(data);

			AssertEquals("DataContextType", DataContextType.ForwardingConsol, manager.DataContextType);
		}

		protected override RecipientRoleType[] SupportedRecipientRoleTypes
		{
			get
			{
				return Enum.GetValues(typeof(RecipientRoleType))
					.Cast<RecipientRoleType>()
					.ToArray();
			}
		}

		protected override string ValidPopulatedUniversalShipmentXML
		{
			get { return string.Empty; }
		}

		protected override VisualizerDocumentData GetNewBusinessObjectForTesting()
		{
			var parent = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			parent[JobConsolSchema.JK_UniqueConsignRef] = "C0000001";

			var documentData = Factory.New<VisualizerDocumentData>();
			documentData.JDD_ParentID = parent.PK;
			documentData.JDD_ParentTableCode = parent.TablePrefix;
			documentData.JDD_Name = "xxx";

			return documentData;
		}
	}
}
