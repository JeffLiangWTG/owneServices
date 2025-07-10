using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Engine;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.Business.Schedule
{
	class ArchiveScheduleTaskValidation : StmScheduleTaskValidation
	{
		public ArchiveScheduleTaskValidation(ArchiveScheduleTask parent)
			: base(parent)
		{ }

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateMaxRunDurationInMinutes();
			ValidateArchiveRecordsOnOrBeforeNumber();
			ValidateArchiveRecordsOnOrBeforeType();
			ValidateDateParameter();
			ValidateArchiveRecordsOnOrBeforeDate();
			ValidateShouldArchiveDeclaration();
		}

		public void ValidateMaxRunDurationInMinutes()
			=> ValidateCalculatedProperty(Parent.MaxRunDurationInMinutesInfo);

		protected virtual void CheckMaxRunDurationInMinutes()
			=> MandatoryValidation.CheckEntered(Parent.MaxRunDurationInMinutesInfo);

		protected override void CheckS5_ParentID()
			=> MandatoryValidation.CheckEntered(Parent.S5_ParentIDInfo);

		protected override void CheckS5_ScheduleType()
		{
			base.CheckS5_ScheduleType();
			MandatoryValidation.CheckEntered(Parent.S5_ScheduleTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.S5_ScheduleTypeInfo);
		}

		protected override void CheckS5_ScheduleDescription()
		{
			base.CheckS5_ScheduleDescription();
			TranslatableDataFieldAttribute.Validate(Parent.S5_ScheduleDescriptionInfo);
			MandatoryValidation.CheckEntered(Parent.S5_ScheduleDescriptionInfo);
		}

		public void ValidateArchiveRecordsOnOrBeforeNumber()
			=> ValidateCalculatedProperty(Parent.ArchiveRecordsOnOrBeforeNumberInfo);

		protected virtual void CheckArchiveRecordsOnOrBeforeNumber()
		{
			if (Parent.IsArchiveRecordsOnOrBeforeRelativeDate)
			{
				MandatoryValidation.CheckEntered(Parent.ArchiveRecordsOnOrBeforeNumberInfo);
			}
		}

		public void ValidateArchiveRecordsOnOrBeforeType()
			=> ValidateCalculatedProperty(Parent.ArchiveRecordsOnOrBeforeTypeInfo);

		protected virtual void CheckArchiveRecordsOnOrBeforeType()
		{
			if (Parent.IsArchiveRecordsOnOrBeforeRelativeDate)
			{
				MandatoryValidation.CheckEntered(Parent.ArchiveRecordsOnOrBeforeTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ArchiveRecordsOnOrBeforeTypeInfo, Parent.ArchiveRecordsOnOrBeforeTypeList);
			}
		}

		public void ValidateDateParameter()
			=> ValidateCalculatedProperty(Parent.DateParameterInfo);

		protected virtual void CheckDateParameter()
		{
			if (!Parent.S5_ScheduleType.IsEmpty)
			{
				var archiveSystemDescriptors = new ArchiveSystemDescriptorLoader().Load();
				var systemDescriptor = archiveSystemDescriptors.FirstOrDefault(descriptor => descriptor.Code == Parent.S5_ScheduleType && descriptor.AllowDateParameterSelection);
				if (systemDescriptor != null)
				{
					Parent.DateParameterInfo.HumanReadableName = (NoResString)"Date Parameter";
					MandatoryValidation.CheckEntered(Parent.DateParameterInfo);
					ListValidation.ErrorIfInvalidCode(Parent.DateParameterInfo, Parent.DateParameterList);
					if (Parent.DateParameter.Equals(DateParameterStrings.GetCode(DateParameterType.JOP)))
					{
						Parent.DateParameterInfo.AddWarning(Res.GetString("0ec0c29e-642a-4dd0-9b8b-5bdf5ceb9eda", $"Please note that you have chosen to process records based on Job Open Date and this is not the default parameter for {systemDescriptor.PresentTenseVerb.ToString().ToLower()}."));
					}
				}
			}
		}

		protected virtual void CheckArchiveRecordsOnOrBeforeDate()
		{
			if (Parent.IsArchiveRecordsOnOrBeforeDate)
			{
				MandatoryValidation.CheckEntered(Parent.ArchiveRecordsOnOrBeforeDateInfo);
			}
		}

		public void ValidateArchiveRecordsOnOrBeforeDate()
			=> ValidateCalculatedProperty(Parent.ArchiveRecordsOnOrBeforeDateInfo);

		public void ValidateShouldArchiveDeclaration()
			=> ValidateCalculatedProperty(Parent.ShouldArchiveDeclarationInfo);

		protected virtual void CheckShouldArchiveDeclaration()
		{
			if (Parent.ShouldArchiveDeclaration)
			{
				Parent.ShouldArchiveDeclarationInfo.AddWarning(Res.GetString("64361C34-1644-481d-818B-C3830F73434F", "The Customs Office of each country/region has strict data retention rules for compliance auditing. In some cases, Customs Office may require jobs to be retained online for more than 5 years. You should consult the relevant compliance legislations before choosing to archive Customs Jobs."));
			}
		}

		new ArchiveScheduleTask Parent
			=> (ArchiveScheduleTask)base.Parent;
	}
}
