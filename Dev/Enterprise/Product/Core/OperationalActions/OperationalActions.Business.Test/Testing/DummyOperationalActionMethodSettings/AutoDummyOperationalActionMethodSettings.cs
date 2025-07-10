using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	[System.CodeDom.Compiler.GeneratedCode("NonPersistentBusinessObjectGenerator", "1.0")]
	public abstract class AutoDummyOperationalActionMethodSettings : Enterprise.Services.OperationalActions.Support.OperationalActionMethodSettings
	{
#region Schema
		public class Schema
		{
			public const string DefaultExcuse = "DefaultExcuse";
			public const int DefaultExcuseMaxLength = 35;
			public const string LockExcuse = "LockExcuse";
		}

#endregion
		protected AutoDummyOperationalActionMethodSettings()
		{
		}

		protected AutoDummyOperationalActionMethodSettings(BusinessObjectFactory factory) : base(factory)
		{
		}

#region DefaultExcuse
		[MaxLength(Schema.DefaultExcuseMaxLength)]
		public virtual ZString DefaultExcuse
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return defaultExcuse;
			}

			set
			{
				CheckMaximumLength(DefaultExcuseInfo, value);
				SetNonPersistentPropertyValue(DefaultExcuseInfo, ref defaultExcuse, value);
				if (IsValidationSuspended)
				{
				}
				else
				{
					Validation.ValidateDefaultExcuse();
				}
			}
		}

		public virtual ZPropertyInfo DefaultExcuseInfo
		{
			get
			{
				return this.GetZPropertyInfo(Schema.DefaultExcuse);
			}
		}

		[System.Diagnostics.DebuggerBrowsableAttribute(System.Diagnostics.DebuggerBrowsableState.Never)]
		ZString defaultExcuse;
#endregion
#region LockExcuse
		public virtual ZBool LockExcuse
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return lockExcuse;
			}

			set
			{
				SetNonPersistentPropertyValue(LockExcuseInfo, ref lockExcuse, value);
				if (IsValidationSuspended)
				{
				}
				else
				{
					Validation.ValidateLockExcuse();
				}
			}
		}

		public virtual ZPropertyInfo LockExcuseInfo
		{
			get
			{
				return this.GetZPropertyInfo(Schema.LockExcuse);
			}
		}

		[System.Diagnostics.DebuggerBrowsableAttribute(System.Diagnostics.DebuggerBrowsableState.Never)]
		ZBool lockExcuse;
#endregion
#region Validation
		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
			base.RunPreSaveValidationCore();
		}

		public DummyOperationalActionMethodSettingsValidation Validation
		{
			get
			{
				return GetNewValidation();
			}
		}

		protected virtual DummyOperationalActionMethodSettingsValidation GetNewValidation()
		{
			return new DummyOperationalActionMethodSettingsValidation(this);
		}

#endregion
#region Xml Serialisation
		protected override void WriteXml(System.Xml.XmlWriter writer)
		{
			writer.WriteStartElement("DefaultExcuse");
			writer.WriteValue(DefaultExcuse);
			writer.WriteEndElement();
			writer.WriteStartElement("LockExcuse");
			writer.WriteValue(LockExcuse.ToString());
			writer.WriteEndElement();
		}

		protected override void ReadXml(System.Xml.XmlReader reader)
		{
			try
			{
				isDeserialising = true;
				reader.ReadStartElement();
				DefaultExcuse = ((CargoWise.Types.ZString)(reader.ReadElementString("DefaultExcuse"))).Left(DefaultExcuseInfo.MaxLength);
				LockExcuse = new ZBool(reader.ReadElementString("LockExcuse"));
				reader.ReadEndElement();
			}
			finally
			{
				isDeserialising = false;
			}
		}

		protected bool IsDeserialising
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				return isDeserialising;
			}
		}

		bool isDeserialising;
#endregion
	}
}
