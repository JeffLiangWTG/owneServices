using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class JobInvoicingDefaultDepartments : RegistryBusinessObjectTemplate, IJobInvoicingDefaultDepartments
	{
		#region Schema

		public abstract class Schema
		{
			public const string ConsolType = "ConsolType";
			public const string Department = "Department";
		}

		#endregion

		public JobInvoicingDefaultDepartments()
		{
		}

		public JobInvoicingDefaultDepartments(bool isDefaulting)
		{
			if (isDefaulting)
			{
				SuspendValidation();
			}
		}

		public JobInvoicingDefaultDepartments(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new JobInvoicingDefaultDepartments(fallbackLevel, null);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateConsolType();
			ValidateDepartment();
		}

		JobInvoicingDefaultDepartmentsCollection ParentCollection
		{
			get
			{
				JobInvoicingDefaultDepartmentsCollection result = null;

				foreach (BusinessObjectCollection collection in ParentCollections)
				{
					if (collection is JobInvoicingDefaultDepartmentsCollection)
					{
						result = (JobInvoicingDefaultDepartmentsCollection)collection;
						break;
					}
				}

				return result;
			}
		}

		#region Bound Properties

		#region Consol Type

		ZString fConsolType;

		[MaxLength(3)]
		[List("ConsolTypes")]
		public ZString ConsolType
		{
			get { return fConsolType; }
			set
			{
				SetNonPersistentPropertyValue(ConsolTypeInfo, ref fConsolType, value);

				if (!IsValidationSuspended)
				{
					RunPreSaveValidationCore();
				}
			}
		}

		public ZPropertyInfo ConsolTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ConsolType); }
		}

		void ValidateConsolType()
		{
			ConsolTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ConsolTypeInfo);
			ListValidation.ErrorIfInvalidCode(ConsolTypeInfo);

			if (ParentCollection != null)
			{
				if (!IsConsolTypesListComplete())
				{
					ConsolTypeInfo.AddError(Res.GetString("3860b83e-2e60-4b17-b915-77c73f44e4cb", "Either all consol types must be entered, or an 'ALL' line should exist."));
				}

				if (!ConsolTypeInfo.HasErrors())
				{
					PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(ConsolTypeInfo, Res.GetString("17cdad1d-ac1b-4912-983e-09697c40570b", "There must be only one '{0}' line.", ConsolType));
				}
			}
		}

		bool IsConsolTypesListComplete()
		{
			bool result = true;

			if (!ParentCollection.ContainsConsolType(Constants.JobInvoicingDefaultDepartmentConsolType.All))
			{
				foreach (ICodeDescription consolTypePair in ConsolTypes)
				{
					if (consolTypePair.Code == Constants.JobInvoicingDefaultDepartmentConsolType.All)
					{
						continue;
					}

					if (!ParentCollection.ContainsConsolType(consolTypePair.Code))
					{
						result = false;
						break;
					}
				}
			}

			return result;
		}

		#endregion

		#region Department

		ZGuid fDepartment;
		[List("Departments")]
		public ZGuid Department
		{
			get { return fDepartment; }
			set
			{
				SetNonPersistentPropertyValue(DepartmentInfo, ref fDepartment, value);

				if (!IsValidationSuspended)
				{
					RunPreSaveValidationCore();
				}
			}
		}

		public ZPropertyInfo DepartmentInfo
		{
			get { return GetZPropertyInfo(Schema.Department); }
		}

		void ValidateDepartment()
		{
			DepartmentInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DepartmentInfo);
			ListValidation.ErrorIfInvalidPK(DepartmentInfo);
		}

		#endregion

		#endregion

		#region Lookups

		public CodeDescriptionPairList ConsolTypes
		{
			get
			{
				return CurrentFactory.GetCachedValue("JobInvoicingDefaultDepartments.ConsolTypes",
					delegate
					{
						CodeDescriptionPairList result = new CodeDescriptionPairList();
						result.AddPair(Constants.JobInvoicingDefaultDepartmentConsolType.All);
						result.AddPair(Constants.JobInvoicingDefaultDepartmentConsolType.NoConsol, Res.GetString("d8cd994e-50e0-4c5a-b006-cb1afbda4e1c", "No consol"));
						result.AddRange(new CodeDescriptionPairList(OLookUpEditType.AgentType));
						result.AddPair(Constants.AgentType.AWBCoload, Constants.AgentTypeDescriptions.AWBCoload);
						return result;
					});
			}
		}

		public GlbDepartmentCollection Departments
		{
			get { return CurrentFactory.GetCachedValue("JobInvoicingDefaultDepartments.Departments", () => new GlbDepartmentCollection(CurrentFactory, NonMiscDepartmentQuery)); }
		}

		ZQuery fNonMiscDepartmentQuery;
		ZQuery NonMiscDepartmentQuery
		{
			get
			{
				if (fNonMiscDepartmentQuery == null)
				{
					fNonMiscDepartmentQuery = new ZDBOnlyQuery(typeof(GlbDepartment));

					string sqlText = string.Format(@"
								{0} = 0
								AND
								(
									{1} IS NULL OR
									{1} NOT IN
									(
										SELECT {2}
										FROM {3}
										WHERE {0} = 1
									)
								)",
												   GlbDepartmentSchema.Constants.GE_Misc, // 0
												   GlbDepartmentSchema.Constants.GE_GE, // 1
												   GlbDepartmentSchema.Constants.PK, // 2
												   GlbDepartmentSchema.Constants.TableName); // 3

					fNonMiscDepartmentQuery.AddFilterAndZSQLParameterCollection(sqlText, new ZSqlParameterCollection());
				}

				return fNonMiscDepartmentQuery;
			}
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.ConsolType, ConsolType);
			writer.WriteElementString(Schema.Department, Department.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ConsolType = reader.ReadElementString(Schema.ConsolType);
			Department = new ZGuid(reader.ReadElementString(Schema.Department));
		}

		#endregion
	}
}
