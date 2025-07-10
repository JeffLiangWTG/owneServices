using System;
using System.Data;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Data
{
	public class TransportBookingTemplateUpgradeTask : EmbeddedUpgradeTask
	{
		public TransportBookingTemplateUpgradeTask() : base(new TransportBookingTemplateDataFile())
		{
		}

		#region Overrides

		protected override void DoInsert(DataRow sourceRow, DataTable targetTable, ref int targetIndex)
		{
			DeleteTransportBookingTemplateAndItsAffiliates(sourceRow);
			base.DoInsert(sourceRow, targetTable, ref targetIndex);
		}

		protected override void DoDelete(DataRow targetRow, ref int targetIndex)
		{
			DeleteTransportBookingTemplateAndItsAffiliates(targetRow);

			// Walk to next row on the target DataSet
			targetIndex++;
		}

		protected override void UpdateColumn(string columnName, DataRow targetRow, DataRow sourceRow)
		{
			if (columnName == DtbBookingInstructionTmplSchema.Constants.K2_DropMode)
			{
				return;
			}

			base.UpdateColumn(columnName, targetRow, sourceRow);
		}

		#endregion

		#region Implementation

		/// <summary>
		/// Attempt to delete a TransportBookingTemplate based on the given PK by deleting its affiliates frist
		/// </summary>
		/// <param name="Pk">Primary Key</param>
		/// <returns>
		/// TRUE : Delete succeeded
		/// FALSE: Delete failed
		/// </returns>
		bool DeleteTransportBookingTemplateAndItsAffiliates(DataRow rowSource)
		{
			if (rowSource.Table.TableName.ToUpper() == DtbBookingTmplSchema.Constants.TableName.ToUpper())
			{
				return DeleteTransportBookingTemplateAndItsInstructions((string)rowSource[DtbBookingTmplSchema.Constants.KT_Code]);
			}
			else if (rowSource.Table.TableName.ToUpper() == DtbBookingInstructionTmplSchema.Constants.TableName.ToUpper())
			{
				return DeleteTransportBookingInstructionTemplate((Guid)rowSource[DtbBookingInstructionTmplSchema.Constants.PK]);
			}
			else
			{
				throw new ArgumentException("Specified Table is not accepted: " + rowSource.Table.TableName);
			}
		}

		internal bool DeleteTransportBookingTemplateAndItsInstructions(Guid templatePK)
		{
			var result = false;
			try
			{
				var delInstructions = "DELETE dbo.DtbBookingInstructionTmpl WHERE K2_KT_BookingTmpl = @PK";
				var cmdLegs = Db.Connection.Command(delInstructions);
				cmdLegs.AddParameterBasedOnDbColumn("@Pk", templatePK, DtbBookingInstructionTmplSchema.K2_KT_BookingTmpl);
				cmdLegs.ExecuteNonQuery();

				var delTemplate = "DELETE dbo.DtbBookingTmpl WHERE KT_PK = @PK";
				var cmdTypes = Db.Connection.Command(delTemplate);
				cmdTypes.AddParameterBasedOnDbColumn("@Pk", templatePK, DtbBookingTmplSchema.PK);
				cmdTypes.ExecuteNonQuery();

				result = true;
			}
			catch (SqlException)
			{
				result = false;
			}

			return result;
		}

		internal bool DeleteTransportBookingTemplateAndItsInstructions(string templateCode)
		{
			var findTemplate = "SELECT KT_PK From dbo.DtbBookingTmpl WHERE KT_Code = @TemplateCode";
			var cmdFindTemplate = Db.Connection.Command(findTemplate);
			cmdFindTemplate.AddParameterBasedOnDbColumn("@TemplateCode", templateCode, DtbBookingTmplSchema.KT_Code);
			var pK = (Guid?)cmdFindTemplate.ExecuteScalar();

			if (pK.HasValue)
			{
				return DeleteTransportBookingTemplateAndItsInstructions(pK.Value);
			}

			return false;
		}

		bool DeleteTransportBookingInstructionTemplate(Guid instructionPK)
		{
			var result = false;
			try
			{
				var delInstructions = "DELETE dbo.DtbBookingInstructionTmpl WHERE K2_PK = @PK";
				var cmdLegs = Db.Connection.Command(delInstructions);
				cmdLegs.AddParameterBasedOnDbColumn("@Pk", instructionPK, DtbBookingInstructionTmplSchema.PK);
				cmdLegs.ExecuteNonQuery();

				result = true;
			}
			catch (SqlException)
			{
				result = false;
			}

			return result;
		}
		#endregion
	}
}
