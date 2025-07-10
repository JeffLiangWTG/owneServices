using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using SharedConstants = WTG.Shared.Dash.Common.Constants;

namespace Enterprise.Dash.Business
{
	[XmlSerializerAssembly("Enterprise.Dash.Business.XmlSerializers")]
	public class WorkflowConfigurationStepCollection : RegistryBusinessObjectCollectionTemplate, ICodeDescriptionPairList
	{
		CodeDescriptionPairListProvider codesProvider;

		public WorkflowConfigurationStepCollection()
		{
		}

		public WorkflowConfigurationStepCollection(CodeDescriptionPairListProvider codesProvider)
		{
			SetCodesProvider(codesProvider);
		}

		public new WorkflowConfigurationStep this[int index]
		{
			get { return (WorkflowConfigurationStep)base[index]; }
		}

		public ICodeDescriptionPairList Codes
		{
			get { return codesProvider.CodeDescriptionPairList; }
		}

		public new WorkflowConfigurationStep AddNew()
		{
			return (WorkflowConfigurationStep)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new WorkflowConfigurationStepCollection(codesProvider);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WorkflowConfigurationStep();
		}

		public void SetCodesProvider(CodeDescriptionPairListProvider value)
		{
			codesProvider = value;
		}

		#region ICodeDescriptionPairList Members

		bool ICodeDescriptionPairList.ContainsCode(object code)
		{
			if (code != null)
			{
				foreach (var selection in this.Cast<WorkflowConfigurationStep>())
				{
					if (selection.Code == code.ToString())
					{
						return true;
					}
				}
			}

			return false;
		}

		string ICodeDescriptionPairList.GetDescriptionFromCode(string code)
		{
			foreach (var selection in this.Cast<WorkflowConfigurationStep>())
			{
				if (selection.Code == code)
				{
					return selection.Description;
				}
			}

			return "";
		}

		#endregion

		public override bool Equals(object obj)
		{
			if (base.Equals(obj))
			{
				return true;
			}

			if (obj is not ICodeDescriptionPairList other)
			{
				return false;
			}

			if (other.Count == 0 && Count == 0)
			{
				return true;
			}

			if (other.Count != Count)
			{
				return false;
			}

			foreach (var selection in this.Cast<WorkflowConfigurationStep>())
			{
				if (!other.ContainsCode(selection.Code))
				{
					return false;
				}
			}

			return true;
		}

		public static bool operator ==(WorkflowConfigurationStepCollection lhs, object rhs)
		{
			if (((object)lhs) == null)
			{
				return rhs == null;
			}

			return lhs.Equals(rhs);
		}

		public static bool operator !=(WorkflowConfigurationStepCollection lhs, object rhs)
		{
			if (((object)lhs) == null)
			{
				return rhs != null;
			}

			return !lhs.Equals(rhs);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 0;
				foreach (var selection in this.AsEnumerable().Cast<WorkflowConfigurationStep>().OrderBy(x => x.Code))
				{
					hash = hash * 139 + selection.Code.GetHashCode();
				}
				return hash;
			}
		}

		protected override bool RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			var elementsList = Elements
				.Cast<WorkflowConfigurationStep>()
				.ToList();

			var duplicateNumbers = elementsList.GroupBy(x => x.Code).Where(g => g.Count() > 1).Select(g => g.Key).ToArray().ToList();

			for (var i = 0; i < elementsList.Count; i++)
			{
				var element = elementsList[i];

				if (duplicateNumbers.Any(e => e == element.Code))
				{
					element.CodeInfo.AddError(ResString.GetMultilingualString("C24480A4-fE63-4B1C-944D-84FFE610B598", "Task {0} can only be added once in a workflow configuration.", element.Code));
				}

				if (element.Code == SharedConstants.DataProcessingType.Code.ProductCodeMatching)
				{
					var ormIndex = elementsList.FindIndex(e => e.Code == SharedConstants.DataProcessingType.Code.OrganizationMatching);

					if (ormIndex < 0 || ormIndex > i)
					{
						element.CodeInfo.AddError(ResString.GetMultilingualString("1EF619AC-4B70-404E-BC6A-1BCE34CB8CA0", "The PCM step must be preceded by the ORM step."));
					}
				}

				var elementCount = elementsList.Count;

				if (element.Code == SharedConstants.DataProcessingType.Code.NotifyDownstreamServices)
				{
					if (i != elementCount - 1)
					{
						element.CodeInfo.AddError(ResString.GetMultilingualString("02830F2F-1C12-4B1A-81F7-C17C48B6E226", "The NDS step must be last in the workflow."));
					}
				}

				if (!elementsList.Any(x => x.Code == SharedConstants.DataProcessingType.Code.NotifyDownstreamServices))
				{
					element.CodeInfo.AddError(ResString.GetMultilingualString("07559104-1BBB-4A81-8577-96047A8E03D6", "The NDS step is mandatory."));
				}
			}

			return true;
		}
	}
}
