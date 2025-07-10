using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Shared
{
	public partial class StmDefaultPrinter : AutoStmDefaultPrinter, IStmDefaultPrinter
	{
		public StmDefaultPrinter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region LoadDefaultPrinter

		public static StmDefaultPrinter LoadDefaultPrinter(BusinessObjectFactory factory, BusinessObject subject, bool shouldRegisterEditableChildObject = true)
		{
			return LoadDefaultPrinter(factory, subject, null, shouldRegisterEditableChildObject);
		}

		public static StmDefaultPrinter LoadDefaultPrinter(BusinessObjectFactory factory, BusinessObject subject, IStmMenuItem menuItem, bool shouldRegisterEditableChildObject = true, IStmMenuTemplatePivot templatePivot = null)
		{
			Argument.NotNull(subject, nameof(subject));

			var result = LoadDefaultPrinterUnsafe(factory, subject.PK, menuItem?.PK, templatePivot?.PK);

			if (shouldRegisterEditableChildObject)
			{
				subject.RegisterEditableChildObject(result);
			}

			return result;
		}

		public static StmDefaultPrinter LoadDefaultPrinterUnsafe(BusinessObjectFactory factory, ZGuid subjectPK, ZGuid? menuItemPK, ZGuid? templatePivotPK)
		{
			Argument.NotNull(factory, nameof(factory));
			var query = new ZQuery();
			query.AddToFilter(StmDefaultPrinterSchema.SDP_SubjectID, subjectPK);
			query.AddToFilter(StmDefaultPrinterSchema.SDP_SU_Document, menuItemPK);
			query.AddToFilter(StmDefaultPrinterSchema.SDP_SI, templatePivotPK);

			return factory.LoadTop1<StmDefaultPrinter>(query); // there should only be one;
		}

		#endregion

		#region LoadOrCreateDefaultPrinter

		public static StmDefaultPrinter LoadOrCreateDefaultPrinter(BusinessObjectFactory factory, BusinessObject subject, bool shouldRegisterEditableChildObject = true)
		{
			return LoadOrCreateDefaultPrinter(factory, subject, null, shouldRegisterEditableChildObject);
		}

		public static StmDefaultPrinter LoadOrCreateDefaultPrinter(BusinessObjectFactory factory, BusinessObject subject, IStmMenuItem menuItem, bool shouldRegisterEditableChildObject = true, IStmMenuTemplatePivot templatePivot = null)
		{
			Argument.NotNull(subject, nameof(subject));
			var result = LoadDefaultPrinter(factory, subject, menuItem, shouldRegisterEditableChildObject, templatePivot) ?? CreateDefaultPrinter(factory, subject, menuItem?.PK, shouldRegisterEditableChildObject, templatePivot?.PK);

			return result;
		}

		public static StmDefaultPrinter LoadOrCreateDefaultPrinterUnsafe(BusinessObjectFactory factory, ZGuid subjectPK, string subjectTablePrefix, ZGuid? menuItemPK, ZGuid? templatePivotPK)
		{
			var result = LoadDefaultPrinterUnsafe(factory, subjectPK, menuItemPK, templatePivotPK) ?? CreateDefaultPrinterUnsafe(factory, subjectPK, subjectTablePrefix, menuItemPK, templatePivotPK);

			return result;
		}

		static StmDefaultPrinter CreateDefaultPrinter(BusinessObjectFactory factory, BusinessObject subject, ZGuid? menuItemPK, bool shouldRegisterEditableChildObject = true, ZGuid? pivotPK = null)
		{
			var result = CreateDefaultPrinterUnsafe(factory, subject.PK, subject.TablePrefix, menuItemPK, pivotPK);
			if (shouldRegisterEditableChildObject)
			{
				subject.RegisterEditableChildObject(result);
			}

			return result;
		}

		static StmDefaultPrinter CreateDefaultPrinterUnsafe(BusinessObjectFactory factory, ZGuid subjectPK, string subjectTablePrefix, ZGuid? menuItemPK, ZGuid? pivotPK)
		{
			var result = factory.New<StmDefaultPrinter>();

			result.SDP_SubjectID = subjectPK;
			result.SDP_SubjectTableCode = subjectTablePrefix;

			if (menuItemPK.HasValue)
			{
				result.SDP_SU_Document = menuItemPK.Value;
			}

			if (pivotPK.HasValue)
			{
				result.SDP_SI = pivotPK.Value;
			}

			return result;
		}

		#endregion

		#region SetPrinterPKOrDeleteIfEmpty

		public static void SetPrinterPKOrDeleteIfEmpty(BusinessObjectFactory factory, BusinessObject subject, ZGuid printerPK, bool suspendValidation = true)
		{
			var existingPrinter = StmDefaultPrinter.LoadDefaultPrinter(factory, subject);
			var previousValue = existingPrinter != null ? existingPrinter.SDP_SQ_Printer : ZGuid.Empty;
			if (previousValue != printerPK)
			{
				// if changing from empty we need to create a new default printer,
				// if changing from non-empty then the printer exists.
				var defaultPrinter = existingPrinter ?? CreateDefaultPrinter(factory, subject, null);

				if (printerPK.IsEmpty)
				{
					defaultPrinter.Delete();
				}
				else
				{
					using (suspendValidation ? defaultPrinter.GetValidationSuspender() : null)
					{
						defaultPrinter.SDP_SQ_Printer = printerPK;
					}
				}
			}
		}

		#endregion

		#region Properties

		#region SDP_SubjectID

		[RelatedBusinessObject("Dummy")]
		[RelatedBusinessObjectTestExclude("In NewWithValidTestData method, system will evaluate this property with a new BizO, creating with reflection")]
		public override ZGuid SDP_SQ_Printer
		{
			get { return base.SDP_SQ_Printer; }
			set { base.SDP_SQ_Printer = value; }
		}

		#endregion

		#endregion
	}
}
