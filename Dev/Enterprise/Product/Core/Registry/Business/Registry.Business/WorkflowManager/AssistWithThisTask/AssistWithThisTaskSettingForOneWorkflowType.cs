using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class AssistWithThisTaskSettingForOneWorkflowType : RegistryBusinessObject
	{
		public AssistWithThisTaskSettingForOneWorkflowType()
		{
		}

		public AssistWithThisTaskSettingForOneWorkflowType(BusinessObjectFactory factory)
			: base(null, factory)
		{
		}

		#region Properties

		#region TaskType

		[List(nameof(TaskTypeList))]
		public ZString TaskType
		{
			get => taskType;
			set
			{
				SetNonPersistentPropertyValue(TaskTypeInfo, ref taskType, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateTaskType();
				}
			}
		}

		ZString taskType;

		public ZPropertyInfo TaskTypeInfo => GetZPropertyInfo(nameof(TaskType));

		#endregion

		#region LowEstimateMinutes

		public ZInt LowEstimateMinutes
		{
			get => lowEstimateMinutes;
			set
			{
				SetNonPersistentPropertyValue(LowEstimateMinutesInfo, ref lowEstimateMinutes, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateLowEstimateMinutes();
				}
			}
		}

		ZInt lowEstimateMinutes;

		public ZPropertyInfo LowEstimateMinutesInfo => GetZPropertyInfo(nameof(LowEstimateMinutes));

		#endregion

		#region VariationFactor

		public ZInt VariationFactor
		{
			get => variationFactor;
			set
			{
				SetNonPersistentPropertyValue(VariationFactorInfo, ref variationFactor, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateVariationFactor();
				}
			}
		}

		ZInt variationFactor;

		public ZPropertyInfo VariationFactorInfo => GetZPropertyInfo(nameof(VariationFactor));

		#endregion

		#endregion

		#region Set Default Value

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			LowEstimateMinutes = 10;
			VariationFactor = 2;
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AssistWithThisTaskSettingForOneWorkflowType(factory);
		}

		#endregion

		#region Xml Serialisation

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);

			if (reader.IsStartElement(nameof(TaskType)))
			{
				TaskType = reader.ReadElementString(nameof(TaskType));
			}
			if (reader.IsStartElement(nameof(LowEstimateMinutes)))
			{
				LowEstimateMinutes = reader.ReadElementContentAsInt();
			}
			if (reader.IsStartElement(nameof(VariationFactor)))
			{
				VariationFactor = reader.ReadElementContentAsInt();
			}
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);

			writer.WriteElementString(nameof(TaskType), TaskType);
			writer.WriteElementString(nameof(LowEstimateMinutes), LowEstimateMinutes.ToString());
			writer.WriteElementString(nameof(VariationFactor), VariationFactor.ToString());
		}

		#endregion

		#region Lookups

		public ICodeDescriptionPairList TaskTypeList
		{
			get
			{
				if (taskTypeList == null)
				{
					taskTypeList = (ICodeDescriptionPairList)WorkflowDataRegistry.Instance.TaskTypes.GetFallBackValueAtAllLevels(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty)?.GetTaskTypesFromWorkflowCode(Code)
								   ?? new CodeDescriptionPairList();
				}

				return taskTypeList;
			}
		}

		ICodeDescriptionPairList taskTypeList;

		#endregion

		#region Validation

		AssistWithThisTaskSettingForOneWorkflowValidation Validation => validation ?? (validation = new AssistWithThisTaskSettingForOneWorkflowValidation(this));
		AssistWithThisTaskSettingForOneWorkflowValidation validation;

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		class AssistWithThisTaskSettingForOneWorkflowValidation : ZValidation
		{
			readonly AssistWithThisTaskSettingForOneWorkflowType parent;

			public AssistWithThisTaskSettingForOneWorkflowValidation(AssistWithThisTaskSettingForOneWorkflowType parent)
				: base(parent)
			{
				this.parent = parent;
			}

			public override void ValidateAll()
			{
				ValidateTaskType();
				ValidateLowEstimateMinutes();
				ValidateVariationFactor();
			}

			public void ValidateTaskType()
			{
				ValidateCalculatedProperty(parent.TaskTypeInfo);
			}

			protected void CheckTaskType()
			{
				ListValidation.ErrorIfInvalidCode(parent.TaskTypeInfo);
			}

			public void ValidateLowEstimateMinutes()
			{
				ValidateCalculatedProperty(parent.LowEstimateMinutesInfo);
			}

			protected void CheckLowEstimateMinutes()
			{
				CompareValidation.CheckWithinRange(parent.LowEstimateMinutesInfo, 0, 999);
			}

			public void ValidateVariationFactor()
			{
				ValidateCalculatedProperty(parent.VariationFactorInfo);
			}

			protected void CheckVariationFactor()
			{
				CompareValidation.CheckWithinRange(parent.VariationFactorInfo, 1, 999);
			}

			public override Type AutoValidationType => typeof(AssistWithThisTaskSettingForOneWorkflowValidation);
		}

		#endregion
	}
}
