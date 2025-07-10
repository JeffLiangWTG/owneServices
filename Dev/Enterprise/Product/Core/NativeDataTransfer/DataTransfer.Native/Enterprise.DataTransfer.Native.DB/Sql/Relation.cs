using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Enterprise.DataTransfer.Native.DB.Keys;
using Enterprise.DataTransfer.Native.Utils.Models;

namespace Enterprise.DataTransfer.Native.DB.Sql
{
	public class Relation : Edge<Table>
	{
		public Relation(Table from, Table to, KeyRelation[] keys)
			: base(from, to)
		{
			Keys = keys;
		}

		public KeyRelation[] Keys;

		public bool IsSelfReferencing { get { return From.Equals(To); } }

		public IEnumerable<Key> GetKeys(Table table)
		{
			if (table == From)
			{
				return Keys.Select(x => x.FromKey);
			}

			if (table == To)
			{
				return Keys.Select(x => x.ToKey);
			}
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Cannot find matching table(Param:{0},From:{1},To:{2})", table.Name, From.Name, To.Name));
		}

		public override Table Mate(Table node)
		{
			if (node == From)
			{
				return To;
			}

			if (node == To)
			{
				return From;
			}

			throw new ArgumentException("Could not find the other end of " + node + " is not belongs to " + this);
		}

		public static Relation Build(params Key[] keys)
		{
			var keyRelations = keys.Select(x => new KeyRelation(x)).ToArray();
			var from = keyRelations[0].FromKey.Table;
			var to = keyRelations[0].FromKey.ReferenceTable;

			return new Relation(from, to, keyRelations);
		}
	}
}
