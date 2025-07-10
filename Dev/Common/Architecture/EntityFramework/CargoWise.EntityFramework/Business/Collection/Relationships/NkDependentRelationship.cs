using System;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	public class NkDependentRelationship : CollectionRelationship
	{
		public NkDependentRelationship(BusinessObject master, Type elementType, ZQuery filter, SchemaStringColumn nkColumn)
			: base(elementType, filter)
		{
			Argument.NotNull(master, "master");
			Argument.NotNull(nkColumn, "nkColumn");

			this.master = master;
			this.NKSchemaColumnInDependent = nkColumn;

			var codeProperty = CodePropertyAttribute.CodePropertyNameFromType(master.GetType());
			Master.ZPropertyInfoHash[codeProperty].ValueChanged += new EventHandler(CodeProperty_ValueChanged);
		}

		#region Master

		public override BusinessObject Master
		{
			get { return master; }
		}
		readonly BusinessObject master;

		ZString MasterCode
		{
			get { return Master.IsDeleted ? ZString.Empty : CodePropertyAttribute.CodeFromBusinessObject(Master); }
		}

		ZBool IsMasterCodeValid
		{
			get { return !MasterCode.IsEmpty; }
		}

		#endregion

		#region Equals / GetHashCode

		public override bool Equals(object obj)
		{
			NkDependentRelationship rhs = obj as NkDependentRelationship;
			bool result = rhs != null;

			result = result && base.Equals(rhs);
			result = result && Master == rhs.Master;
			result = result && ElementType == rhs.ElementType;
			return result;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode() ^ Master.PK.GetHashCode();
		}

		#endregion

		#region Filter

		readonly SchemaStringColumn NKSchemaColumnInDependent;

		void CodeProperty_ValueChanged(object sender, EventArgs e)
		{
			OnRelationshipFilterChanged(EventArgs.Empty);
		}

		protected override ZQuery RelationshipFilterCore
		{
			get
			{
				if (IsMasterCodeValid)
				{
					var query = new ZQuery(NKSchemaColumnInDependent, MasterCode);
					query.FetchOnlyFromLocalCache = !master.IsInDatabase;
					return query;
				}
				else
				{
					return ZQuery.NoResultQuery;
				}
			}
		}

		protected override bool SupportsAddToRelationshipCore()
		{
			return IsMasterCodeValid;
		}

		protected override void AddToRelationship(BusinessObject businessObject)
		{
			if (!IsMasterCodeValid)
			{
				string message = string.Format((NoResString)@"Master Code is not valid. Natural Key needs to be set before adding children. Relationship type: {0} Added bizo type: {1}", GetType().FullName, businessObject.GetType().FullName);

				ErrorReporter.ReportOnce("AddToRelationshipWithInvalidMasterCode", message);
				throw new InvalidOperationException(message);
			}

			businessObject[NKSchemaColumnInDependent] = MasterCode;
		}

		protected override void RemoveFromRelationship(BusinessObject businessObject)
		{
			businessObject[NKSchemaColumnInDependent] = ZString.Empty;
		}

		#endregion
	}
}
