using CargoWise.EntityFramework.Testing;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class OperationalActionFieldDescriptorLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestFieldNames()
		{
			AssertNotNull("precondition: Z0_VarCharMax", Context.FieldSupporters["Z0_VarCharMax"]);
			AssertNotNull("precondition: Z0_Date", Context.FieldSupporters["Z0_Date"]);
			CodeDescriptionPairList fieldNames = FieldDescriptor.Lookups.FieldNames;
			AssertNotNull("Z0_VarCharMax", fieldNames["Z0_VarCharMax"]);
			AssertNotNull("Z0_Date", fieldNames["Z0_Date"]);
		}

		public void TestDefaultStrategy_List()
		{
			FieldDescriptor.FieldName = "";
			AssertMultilineASCIIEquals("", "", FieldDescriptor.Lookups.DefaultStrategy_List.ElementsAsString);
			FieldDescriptor.FieldName = DummyBusinessObject.Schema.Z0_Code;
			AssertMultilineASCIIEquals("", "FXD - Fixed Text", FieldDescriptor.Lookups.DefaultStrategy_List.ElementsAsString);
			FieldDescriptor.FieldName = DummyBusinessObject.Schema.Z0_Date;
			AssertMultilineASCIIEquals("", "FXD - Fixed Date\nDAY - Today + Days", FieldDescriptor.Lookups.DefaultStrategy_List.ElementsAsString);
		}

		#region Implementation
		OperationalActionFieldDescriptor FieldDescriptor
		{
			get
			{
				return fieldDescriptor ?? (fieldDescriptor = Action.FieldDescriptors.AddNew());
			}
		}

		OperationalActionFieldDescriptor fieldDescriptor;
		OperationalAction Action
		{
			get
			{
				if (action == null)
				{
					action = Factory.New<OperationalAction>();
					action.Context = Context;
				}

				return action;
			}
		}

		OperationalAction action;
		OperationalActionContext Context
		{
			get
			{
				return context ?? (context = new OperationalActionContext(ActionSupporter, "Module Name"));
			}
		}

		OperationalActionContext context;
		OperationalActionSupporter ActionSupporter
		{
			get
			{
				return actionSupporter ?? (actionSupporter = new MockOperationalActionSupportable().OperationalActionSupporter);
			}
		}

		OperationalActionSupporter actionSupporter;
		#endregion
	}
}
