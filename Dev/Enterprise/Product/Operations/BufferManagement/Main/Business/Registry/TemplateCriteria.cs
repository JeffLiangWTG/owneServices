using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business
{
	[XmlSerializerAssembly("Enterprise.BufferManagement.Business.XmlSerializers")]
	public class TemplateCriteria : RegistryBusinessObject
	{
		public TemplateCriteria()
		{
		}

		public TemplateCriteria(BusinessObjectFactory factory)
			: base(null, factory)
		{
		}

		#region Properties

		public ZBool Enabled
		{
			get => enabled;
			set
			{
				SetNonPersistentPropertyValue(EnabledInfo, ref enabled, value);
			}
		}
		ZBool enabled;

		public ZString Criterion1Label { get => ObjectFactory.Get<ISelectionCriteriaLookup>().GetCriterionLabel(1); }
		public ZString Criterion2Label { get => ObjectFactory.Get<ISelectionCriteriaLookup>().GetCriterionLabel(2); }
		public ZString Criterion3Label { get => ObjectFactory.Get<ISelectionCriteriaLookup>().GetCriterionLabel(3); }
		public ZString Criterion4Label { get => ObjectFactory.Get<ISelectionCriteriaLookup>().GetCriterionLabel(4); }
		public ZString Criterion5Label { get => ObjectFactory.Get<ISelectionCriteriaLookup>().GetCriterionLabel(5); }

		[List(nameof(Criterion1List))]
		public ZString Criterion1
		{
			get => criterion1;
			set
			{
				SetNonPersistentPropertyValue(Criterion1Info, ref criterion1, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCriterion1();
				}
			}
		}
		ZString criterion1;

		[List(nameof(Criterion2List))]
		public ZString Criterion2
		{
			get => criterion2;
			set
			{
				SetNonPersistentPropertyValue(Criterion2Info, ref criterion2, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCriterion2();
				}
			}
		}
		ZString criterion2;

		[List(nameof(Criterion3List))]
		public ZString Criterion3
		{
			get => criterion3;
			set
			{
				SetNonPersistentPropertyValue(Criterion3Info, ref criterion3, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCriterion3();
				}
			}
		}
		ZString criterion3;

		[List(nameof(Criterion4List))]
		public ZString Criterion4
		{
			get => criterion4;
			set
			{
				SetNonPersistentPropertyValue(Criterion4Info, ref criterion4, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCriterion4();
				}
			}
		}
		ZString criterion4;

		[List(nameof(Criterion5List))]
		public ZString Criterion5
		{
			get => criterion5;
			set
			{
				SetNonPersistentPropertyValue(Criterion5Info, ref criterion5, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCriterion5();
				}
			}
		}
		ZString criterion5;

		public ICodeDescriptionPairList Criterion1List
		{
			get => ObjectFactory.Get<ISelectionCriteriaLookup>().GetCriterionList(null, null, null, null);
		}
		public ICodeDescriptionPairList Criterion2List
		{
			get => ObjectFactory.Get<ISelectionCriteriaLookup>().GetCriterionList(criterion1, null, null, null);
		}
		public ICodeDescriptionPairList Criterion3List
		{
			get => ObjectFactory.Get<ISelectionCriteriaLookup>().GetCriterionList(criterion1, criterion2, null, null);
		}
		public ICodeDescriptionPairList Criterion4List
		{
			get => ObjectFactory.Get<ISelectionCriteriaLookup>().GetCriterionList(criterion1, criterion2, criterion3, null);
		}
		public ICodeDescriptionPairList Criterion5List
		{
			get => ObjectFactory.Get<ISelectionCriteriaLookup>().GetCriterionList(criterion1, criterion2, criterion3, criterion4);
		}

		public ZPropertyInfo EnabledInfo => GetZPropertyInfo(nameof(Enabled));
		public ZPropertyInfo Criterion1Info => GetZPropertyInfo(nameof(Criterion1));
		public ZPropertyInfo Criterion2Info => GetZPropertyInfo(nameof(Criterion2));
		public ZPropertyInfo Criterion3Info => GetZPropertyInfo(nameof(Criterion3));
		public ZPropertyInfo Criterion4Info => GetZPropertyInfo(nameof(Criterion4));
		public ZPropertyInfo Criterion5Info => GetZPropertyInfo(nameof(Criterion5));

		#endregion

		#region Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TemplateCriteria(factory);
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			Enabled = ((XmlReaderWrapper)reader).ReadElementStringAsZBool(nameof(Enabled));
			Criterion1 = reader.ReadElementString(nameof(Criterion1));
			Criterion2 = reader.ReadElementString(nameof(Criterion2));
			Criterion3 = reader.ReadElementString(nameof(Criterion3));
			Criterion4 = reader.ReadElementString(nameof(Criterion4));
			Criterion5 = reader.ReadElementString(nameof(Criterion5));
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			writer.WriteElementString(nameof(Enabled), Enabled.ToString());
			writer.WriteElementString(nameof(Criterion1), Criterion1);
			writer.WriteElementString(nameof(Criterion2), Criterion2);
			writer.WriteElementString(nameof(Criterion3), Criterion3);
			writer.WriteElementString(nameof(Criterion4), Criterion4);
			writer.WriteElementString(nameof(Criterion5), Criterion5);
		}

		#endregion

		#region Validation

		TemplateCriteriaValidation Validation => validation ?? (validation = new TemplateCriteriaValidation(this));
		TemplateCriteriaValidation validation;

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
		}

		class TemplateCriteriaValidation : ZValidation
		{
			readonly TemplateCriteria parent;

			public TemplateCriteriaValidation(TemplateCriteria parent)
				: base(parent)
			{
				this.parent = parent;
			}

			public override void ValidateAll()
			{
				ValidateCriterion1();
				ValidateCriterion2();
				ValidateCriterion3();
				ValidateCriterion4();
				ValidateCriterion5();
			}

			public void ValidateCriterion1()
			{
				parent.Criterion1Info.ClearAllNotifications();
				ListValidation.ErrorIfInvalidCode(parent.Criterion1Info, parent.Criterion1List);
			}

			public void ValidateCriterion2()
			{
				parent.Criterion2Info.ClearAllNotifications();
				ListValidation.ErrorIfInvalidCode(parent.Criterion2Info, parent.Criterion2List);
			}

			public void ValidateCriterion3()
			{
				parent.Criterion3Info.ClearAllNotifications();
				ListValidation.ErrorIfInvalidCode(parent.Criterion3Info, parent.Criterion3List);
			}

			public void ValidateCriterion4()
			{
				parent.Criterion4Info.ClearAllNotifications();
				ListValidation.ErrorIfInvalidCode(parent.Criterion4Info, parent.Criterion4List);
			}

			public void ValidateCriterion5()
			{
				parent.Criterion5Info.ClearAllNotifications();
				ListValidation.ErrorIfInvalidCode(parent.Criterion5Info, parent.Criterion5List);
			}

			public override Type AutoValidationType => typeof(TemplateCriteriaValidation);
		}

		#endregion
	}
}
