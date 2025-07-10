using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.Module.Testing
{
	public class TemporaryStorageFilterControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestColumns()
		{
			using (var form = new ZForm())
			{
				var filterControl = new TemporaryStorageFilterControl(new CusTempStorageJobHeaderCollection<CusTempStorageJobHeader>(Factory, GlbBranch.CurrentBranch), new TemporaryStorageFilterStripBusinessObject());
				form.Controls.Add((filterControl));
				form.Show();

				var columnStyles = filterControl.Grid.ColumnStyles;
				CombineAssertions(() =>
				{
					Assert(CusTempStorageJobHeaderSchema.Constants.SJH_JobReference, columnStyles.OfType<ZGridColumnInfo>().Any(info => info.ColumnName == CusTempStorageJobHeaderSchema.Constants.SJH_JobReference && info.CaptionResourceString.Caption == "Job Number / DDT Number"));
					Assert("Customer.OH_Code", columnStyles.OfType<ZGridColumnInfo>().Any(info => info.ColumnName == "Customer+OH_Code" && info.CaptionResourceString.Caption == "Customer Code"));
					Assert("Customer.OH_FullName", columnStyles.OfType<ZGridColumnInfo>().Any(info => info.ColumnName == "Customer+OH_FullName" && info.CaptionResourceString.Caption == "Customer Name"));
					Assert("Presenter.EffectiveCompanyName", columnStyles.OfType<ZGridColumnInfo>().Any(info => info.ColumnName == "Presenter+EffectiveCompanyName" && info.CaptionResourceString.Caption == "Presenter"));
					Assert("Representative.EffectiveCompanyName", columnStyles.OfType<ZGridColumnInfo>().Any(info => info.ColumnName == "Representative+EffectiveCompanyName" && info.CaptionResourceString.Caption == "Representative"));
					Assert(CusTempStorageJobHeaderSchema.Constants.SJH_ArrivalDate, columnStyles.OfType<ZGridColumnInfo>().Any(info => info.ColumnName == CusTempStorageJobHeaderSchema.Constants.SJH_ArrivalDate && info.CaptionResourceString.Caption == "Arrival Date"));
					Assert(CusTempStorageJobHeaderSchema.Constants.SJH_PresentationDate, columnStyles.OfType<ZGridColumnInfo>().Any(info => info.ColumnName == CusTempStorageJobHeaderSchema.Constants.SJH_PresentationDate && info.CaptionResourceString.Caption == "Presentation Date"));
					Assert(CusTempStorageJobHeaderSchema.Constants.SJH_NCTSFlag, columnStyles.OfType<ZGridColumnInfo>().Any(info => info.ColumnName == CusTempStorageJobHeaderSchema.Constants.SJH_NCTSFlag && info.CaptionResourceString.Caption == "NCTS"));
					Assert(CusTempStorageJobHeaderSchema.Constants.SJH_TransportMode, columnStyles.OfType<ZGridColumnInfo>().Any(info => info.ColumnName == CusTempStorageJobHeaderSchema.Constants.SJH_TransportMode && info.CaptionResourceString.Caption == "Transport Mode"));
					Assert(CusTempStorageJobHeaderSchema.Constants.SJH_CustomsOffice, columnStyles.OfType<ZGridColumnInfo>().Any(info => info.ColumnName == CusTempStorageJobHeaderSchema.Constants.SJH_CustomsOffice && info.CaptionResourceString.Caption == "Customs Office"));
					Assert(CusTempStorageJobHeaderSchema.Constants.SJH_RL_NKLoading, columnStyles.OfType<ZGridColumnInfo>().Any(info => info.ColumnName == CusTempStorageJobHeaderSchema.Constants.SJH_RL_NKLoading && info.CaptionResourceString.Caption == "Loading"));
					Assert(CusTempStorageJobHeaderSchema.Constants.SJH_CustomsOfficeOfEntryIntoEU, columnStyles.OfType<ZGridColumnInfo>().Any(info => info.ColumnName == CusTempStorageJobHeaderSchema.Constants.SJH_CustomsOfficeOfEntryIntoEU && info.CaptionResourceString.Caption == "Office of Entry"));
					Assert(CusTempStorageJobHeaderSchema.Constants.SJH_PreviousReferenceType, columnStyles.OfType<ZGridColumnInfo>().Any(info => info.ColumnName == CusTempStorageJobHeaderSchema.Constants.SJH_PreviousReferenceType && info.CaptionResourceString.Caption == "Previous Ref Type" && info.GroupName.ToString().Contains("Previous Ref")));
					Assert(CusTempStorageJobHeaderSchema.Constants.SJH_PreviousReferenceNumber, columnStyles.OfType<ZGridColumnInfo>().Any(info => info.ColumnName == CusTempStorageJobHeaderSchema.Constants.SJH_PreviousReferenceNumber && info.CaptionResourceString.Caption == "Previous Ref Number" && info.GroupName.ToString().Contains("Previous Ref")));
					Assert(CusTempStorageJobHeaderSchema.Constants.SJH_GB, columnStyles.OfType<ZGridColumnInfo>().Any(info => info.ColumnName == CusTempStorageJobHeaderSchema.Constants.SJH_GB && info.CaptionResourceString.Caption == "Branch"));
					Assert(CusTempStorageJobHeaderSchema.Constants.SJH_TempStorageEndDateUtc, columnStyles.OfType<ZGridColumnInfo>().Any(info => info.ColumnName == CusTempStorageJobHeaderSchema.Constants.SJH_TempStorageEndDateUtc && info.CaptionResourceString.Caption == "End Date"));
					Assert(CusTempStorageJobHeaderSchema.Constants.SJH_AppCode, columnStyles.OfType<ZGridColumnInfo>().Any(info => info.ColumnName == CusTempStorageJobHeaderSchema.Constants.SJH_AppCode && info.CaptionResourceString.Caption == "DDT Type"));
					Assert(CusTempStorageJobHeaderSchema.Constants.SJH_CustomsProfile, columnStyles.OfType<ZGridColumnInfo>().Any(info => info.ColumnName == CusTempStorageJobHeaderSchema.Constants.SJH_CustomsProfile && info.CaptionResourceString.Caption == "Authorization Number"));
					Assert(CusTempStorageJobHeaderSchema.Constants.SJH_IsExaminationExpected, columnStyles.OfType<ZGridColumnInfo>().Any(info => info.ColumnName == CusTempStorageJobHeaderSchema.Constants.SJH_IsExaminationExpected && info.CaptionResourceString.Caption == "Examination Expected"));
					Assert(CusTempStorageJobHeaderSchema.Constants.SJH_IsSameConditionExpected, columnStyles.OfType<ZGridColumnInfo>().Any(info => info.ColumnName == CusTempStorageJobHeaderSchema.Constants.SJH_IsSameConditionExpected && info.CaptionResourceString.Caption == "Same State Expected"));
					Assert("SJH_GuaranteeNumber", columnStyles.OfType<ZGridColumnInfo>().Any(info => info.ColumnName == "SJH_GuaranteeNumber" && info.CaptionResourceString.Caption == "Guarantee Number"));
					Assert("SJH_GuaranteeDescription", columnStyles.OfType<ZGridColumnInfo>().Any(info => info.ColumnName == "SJH_GuaranteeDescription" && info.CaptionResourceString.Caption == "Guarantee Description"));
				});
			}
		}
	}
}
