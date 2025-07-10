using System;
using System.Globalization;
using System.Reflection;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.StabilityChecker
{
	public enum StabilityResultLevel
	{
		/// <summary>
		/// Indicates there are no problems
		/// </summary>
		Healthy = 0,
		/// <summary>
		/// Indicates the system is working, but at a reduced capacity
		/// </summary>
		Warning = 1,
		/// <summary>
		/// Indicates a complete failure of the system
		/// </summary>
		Critical = 2,
		/// <summary>
		/// An exception was thrown whilst checking the stability of the system
		/// </summary>
		Exception = 3
	}

	/// <summary>
	/// Describes the result of a stability check
	/// </summary>
	[XmlSerializerAssembly("Enterprise.StabilityChecker.XmlSerializers")]
	public sealed class StabilityResult : RegistryBusinessObjectTemplate
	{
		public StabilityResult()
			: this(false)
		{ }

		public StabilityResult(StabilityResultLevel stabilityLevel, string description, bool isUserRelatedNotification = false)
			: this(stabilityLevel, description, null, isUserRelatedNotification)
		{ }

		public StabilityResult(StabilityResultLevel stabilityLevel, string description, Type stabilityResultHelper, bool isUserRelatedNotification = false)
			: this(isUserRelatedNotification)
		{
			StabilityLevel = stabilityLevel;
			Description = description;
			StabilityResultHelper = stabilityResultHelper;
		}

		public StabilityResult(bool isUserRelatedNotification)
		{
			IsUserRelatedNotification = isUserRelatedNotification;
		}

		/// <summary>
		/// The relative health level returned from the check
		/// </summary>
		public StabilityResultLevel StabilityLevel
		{
			get;
			set;
		}

		public ZString StabilityLevelText
		{
			get { return StabilityLevel.ToString(); }
		}

		/// <summary>
		/// A full text description of the result
		/// </summary>
		public ZString Description
		{
			get;
			set;
		}

		public bool IsUserRelatedNotification { get; set; }

		[XmlIgnore]
		public Type StabilityResultHelper
		{
			get
			{
				Type type = null;
				if (!StabilityResultHelperAssemblyName.IsEmpty && !StabilityResultHelperFullClassName.IsEmpty)
				{
					try
					{
						type = Assembly.Load(StabilityResultHelperAssemblyName).GetType(StabilityResultHelperFullClassName);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
					}
				}
				return type;
			}

			set
			{
				if (value == null)
				{
					StabilityResultHelperAssemblyName = null;
					StabilityResultHelperFullClassName = null;
				}
				else
				{
					StabilityResultHelperAssemblyName = value.Assembly.GetName().Name;
					StabilityResultHelperFullClassName = value.FullName;
				}
			}
		}

		[XmlElement("HelperAsm")]
		public ZString StabilityResultHelperAssemblyName { get; set; }

		[XmlElement("HelperType")]
		public ZString StabilityResultHelperFullClassName { get; set; }

		public override bool Equals(object obj)
		{
			return obj is StabilityResult &&
				((StabilityResult)obj).StabilityLevel == this.StabilityLevel &&
				((StabilityResult)obj).Description == this.Description &&
				((StabilityResult)obj).StabilityResultHelperAssemblyName == this.StabilityResultHelperAssemblyName &&
				((StabilityResult)obj).StabilityResultHelperFullClassName == this.StabilityResultHelperFullClassName &&
				((StabilityResult)obj).IsUserRelatedNotification == this.IsUserRelatedNotification;
		}

		public override int GetHashCode()
		{
			return this.Description.GetHashCode();
		}

		protected override RegistryBusinessObjectTemplate GetClone(ZArchitecture.Environment.FallbackLevel fallbackLevel, CargoWise.EntityFramework.BusinessObjectFactory factory)
		{
			return new StabilityResult();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			StabilityLevel = (StabilityResultLevel)Enum.Parse(typeof(StabilityResultLevel), reader.ReadElementString(Schema.StabilityLevel));
			Description = reader.ReadElementString(Schema.Description);
			StabilityResultHelperAssemblyName = reader.ReadElementString(Schema.StabilityResultHelperAssemblyName);
			StabilityResultHelperFullClassName = reader.ReadElementString(Schema.StabilityResultHelperFullClassName);

			try
			{
				IsUserRelatedNotification = Convert.ToBoolean(reader.ReadElementString(Schema.IsUserRelatedNotification), CultureInfo.InvariantCulture);
			}
			catch (FormatException)
			{
				IsUserRelatedNotification = false;
			}
			catch
			{
				throw;
			}
		}

		public override string ToString() => $"{StabilityLevel} - {Description}";

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			writer.WriteElementString(Schema.StabilityLevel, StabilityLevel.ToString());
			writer.WriteElementString(Schema.Description, Description);
			writer.WriteElementString(Schema.StabilityResultHelperAssemblyName, StabilityResultHelperAssemblyName);
			writer.WriteElementString(Schema.StabilityResultHelperFullClassName, StabilityResultHelperFullClassName);
			writer.WriteElementString(Schema.IsUserRelatedNotification, IsUserRelatedNotification.ToString());
		}

		static class Schema
		{
			internal const string StabilityLevel = "StabilityLevel";
			internal const string Description = "Description";
			internal const string StabilityResultHelperAssemblyName = "HelperAsm";
			internal const string StabilityResultHelperFullClassName = "HelperType";
			internal const string IsUserRelatedNotification = "UserRelatedNotification";
		}
	}
}
