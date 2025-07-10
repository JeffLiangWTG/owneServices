using System.Linq;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class QueryPlanalyzerTest : TestCase
	{
		public void TestAnalyzeQueryPlan1()
		{
			var planalyzer = new QueryPlanalyzer(QueryPlanXml1);

			AssertEquals("SELECT COUNT(*) FROM dbo.GlbStaff WHERE GS_Code = 'DJT'", planalyzer.SqlStatement);
			AssertEquals(0, planalyzer.TableScans.Count());
			AssertEquals(1, planalyzer.IndexSeeks.Count());
			AssertEquals(0, planalyzer.IndexScans.Count());
			AssertEquals(0, planalyzer.RowIDLookups.Count());
			AssertNotNull(planalyzer.QueryPlan);
			AssertEquals(0, planalyzer.OtherQueryPlans.Count());

			var indexDetails = planalyzer.IndexSeeks.Single();

			CombineAssertions(() =>
			{
				AssertEquals("GlbStaff", indexDetails.TableName);
				AssertEquals("NR_UC__GS_Code", indexDetails.IndexName);
				AssertEquals("Clustered", indexDetails.IndexKind);
				AssertEquals("GS_Code", indexDetails.ColumnName);
			});

			CombineAssertions(() =>
			{
				AssertEquals(16, planalyzer.QueryPlan.CachedPlanSize);
				AssertEquals(1, planalyzer.QueryPlan.CompileTime);
				AssertEquals(1, planalyzer.QueryPlan.CompileCPU);
				AssertEquals(248, planalyzer.QueryPlan.CompileMemory);
			});
		}

		public void TestAnalyzeQueryPlan2()
		{
			var planalyzer = new QueryPlanalyzer(QueryPlanXml2);

			AssertEquals("SELECT COUNT(*) FROM dbo.ProcessTasks WHERE P9_GS_NKAssignedStaffMember = 'DJT'", planalyzer.SqlStatement);
			AssertEquals(0, planalyzer.TableScans.Count());
			AssertEquals(1, planalyzer.IndexSeeks.Count());
			AssertEquals(0, planalyzer.IndexScans.Count());
			AssertEquals(0, planalyzer.RowIDLookups.Count());
			AssertNotNull(planalyzer.QueryPlan);
			AssertEquals(0, planalyzer.OtherQueryPlans.Count());

			AssertEquals(3, planalyzer.Elements.Count);

			AssertEquals("Compute Scalar", planalyzer.Elements[0].PhysicalOperator);
			AssertEquals(0M, planalyzer.Elements[0].EstimateCPU);
			AssertEquals(0M, planalyzer.Elements[0].EstimateIO);
			AssertEquals(0, planalyzer.Elements[0].EstimateCostPercent);

			AssertEquals("Stream Aggregate", planalyzer.Elements[1].PhysicalOperator);
			AssertEquals(9.257E-06M, planalyzer.Elements[1].EstimateCPU);
			AssertEquals(0M, planalyzer.Elements[1].EstimateIO);
			AssertEquals(0, planalyzer.Elements[1].EstimateCostPercent);

			AssertEquals("Index Seek", planalyzer.Elements[2].PhysicalOperator);
			AssertEquals(0.000173054M, planalyzer.Elements[2].EstimateCPU);
			AssertEquals(0.003125M, planalyzer.Elements[2].EstimateIO);
			AssertEquals(100, planalyzer.Elements[2].EstimateCostPercent);

			var indexDetails = planalyzer.IndexSeeks.Single();

			CombineAssertions(() =>
			{
				AssertEquals("ProcessTasks", indexDetails.TableName);
				AssertEquals("NR_RX__P9_GS_NKAssignedStaffMember_P9_Status", indexDetails.IndexName);
				AssertEquals("NonClustered", indexDetails.IndexKind);
				AssertEquals("P9_GS_NKAssignedStaffMember", indexDetails.ColumnName);
			});

			CombineAssertions(() =>
			{
				AssertEquals(16, planalyzer.QueryPlan.CachedPlanSize);
				AssertEquals(13, planalyzer.QueryPlan.CompileTime);
				AssertEquals(13, planalyzer.QueryPlan.CompileCPU);
				AssertEquals(544, planalyzer.QueryPlan.CompileMemory);
			});
		}

		public void TestAnalyzeQueryPlan3()
		{
			var planalyzer = new QueryPlanalyzer(QueryPlanXml3);

			AssertEquals("select top 1 GS_Code from dbo.GlbStaff", planalyzer.SqlStatement);
			AssertEquals(0, planalyzer.TableScans.Count());
			AssertEquals(0, planalyzer.IndexSeeks.Count());
			AssertEquals(1, planalyzer.IndexScans.Count());
			AssertEquals(0, planalyzer.RowIDLookups.Count());
			AssertNotNull(planalyzer.QueryPlan);
			AssertEquals(0, planalyzer.OtherQueryPlans.Count());

			var indexDetails = planalyzer.IndexScans.Single();

			CombineAssertions(() =>
			{
				AssertEquals("GlbStaff", indexDetails.TableName);
				AssertEquals("FK_RX__GS_PER", indexDetails.IndexName);
				AssertEquals("NonClustered", indexDetails.IndexKind);
				AssertEquals("GS_Code", indexDetails.ColumnName);
			});

			CombineAssertions(() =>
			{
				AssertEquals(16, planalyzer.QueryPlan.CachedPlanSize);
				AssertEquals(0, planalyzer.QueryPlan.CompileTime);
				AssertEquals(0, planalyzer.QueryPlan.CompileCPU);
				AssertEquals(216, planalyzer.QueryPlan.CompileMemory);
			});
		}

		public void TestAnalyzeQueryPlan4()
		{
			var planalyzer = new QueryPlanalyzer(QueryPlanXml4);

			AssertEquals("SELECT COUNT(*) FROM dbo.ProcessHeader WHERE FH_CompletionStatement = 'Donald J. Trump'", planalyzer.SqlStatement);
			AssertEquals(1, planalyzer.TableScans.Count());
			AssertEquals(0, planalyzer.IndexSeeks.Count());
			AssertEquals(0, planalyzer.IndexScans.Count());
			AssertEquals(0, planalyzer.RowIDLookups.Count());
			AssertNotNull(planalyzer.QueryPlan);
			AssertEquals(0, planalyzer.OtherQueryPlans.Count());

			var indexDetails = planalyzer.TableScans.Single();

			CombineAssertions(() =>
			{
				AssertEquals("ProcessHeader", indexDetails.TableName);
				AssertEquals("FH_CompletionStatement", indexDetails.ColumnName);
			});

			CombineAssertions(() =>
			{
				AssertEquals(16, planalyzer.QueryPlan.CachedPlanSize);
				AssertEquals(3, planalyzer.QueryPlan.CompileTime);
				AssertEquals(3, planalyzer.QueryPlan.CompileCPU);
				AssertEquals(320, planalyzer.QueryPlan.CompileMemory);
			});
		}

		public void TestAnalyzeQueryPlan5()
		{
			var planalyzer = new QueryPlanalyzer(QueryPlanXml5);

			AssertEquals("SELECT [P9_Type],[P9_Description] FROM [ProcessTasks] WHERE [P9_GS_NKAssignedStaffMember]=@1", planalyzer.SqlStatement);
			AssertEquals(0, planalyzer.TableScans.Count());
			AssertEquals(1, planalyzer.IndexSeeks.Count());
			AssertEquals(0, planalyzer.IndexScans.Count());
			AssertEquals(1, planalyzer.RowIDLookups.Count());
			AssertNotNull(planalyzer.QueryPlan);
			AssertEquals(0, planalyzer.OtherQueryPlans.Count());

			var ridLookupDetails = planalyzer.RowIDLookups.Single();

			CombineAssertions(() =>
			{
				AssertEquals(2, ridLookupDetails.OutputList.Count);

				AssertEquals("ProcessTasks", ridLookupDetails.OutputList[0].TableName);
				AssertEquals("P9_Type", ridLookupDetails.OutputList[0].ColumnName);
				AssertEquals("ProcessTasks", ridLookupDetails.OutputList[1].TableName);
				AssertEquals("P9_Description", ridLookupDetails.OutputList[1].ColumnName);
			});

			CombineAssertions(() =>
			{
				AssertEquals(24, planalyzer.QueryPlan.CachedPlanSize);
				AssertEquals(2, planalyzer.QueryPlan.CompileTime);
				AssertEquals(2, planalyzer.QueryPlan.CompileCPU);
				AssertEquals(520, planalyzer.QueryPlan.CompileMemory);
			});
		}

		public void TestAnalyzeQueryPlan6_IndexSeekNumbers()
		{
			var planalyzer = new QueryPlanalyzer(QueryPlanXml6);

			AssertEquals("SELECT COUNT(*) FROM dbo.AccTransactionHeader WHERE AH_ConsolidatedInvoiceRef IN ('A', 'B', 'C', 'S00001561', 'S00001413')", planalyzer.SqlStatement);
			AssertEquals(0, planalyzer.TableScans.Count());
			AssertEquals(1, planalyzer.IndexSeeks.Count());
			AssertEquals(0, planalyzer.IndexScans.Count());
			AssertEquals(0, planalyzer.RowIDLookups.Count());
			AssertNotNull(planalyzer.QueryPlan);
			AssertEquals(0, planalyzer.OtherQueryPlans.Count());

			var indexDetails = planalyzer.IndexSeeks.Single();

			CombineAssertions(() =>
			{
				AssertEquals("AccTransactionHeader", indexDetails.TableName);
				AssertEquals("NR_RX__AH_ConsolidatedInvoiceRef", indexDetails.IndexName);
				AssertEquals("NonClustered", indexDetails.IndexKind);
				AssertEquals("AH_ConsolidatedInvoiceRef", indexDetails.ColumnName);

				AssertEquals("ActualExecutions", 3L, indexDetails.ActualExecutions);
				AssertEquals("ActualScans", 20L, indexDetails.ActualScans);
				AssertEquals("ActualLogicalReads", 14L, indexDetails.ActualLogicalReads);
				AssertEquals("ActualRowsRead", 11L, indexDetails.ActualRowsRead);
			});

			CombineAssertions(() =>
			{
				AssertEquals(24, planalyzer.QueryPlan.CachedPlanSize);
				AssertEquals(2, planalyzer.QueryPlan.CompileTime);
				AssertEquals(2, planalyzer.QueryPlan.CompileCPU);
				AssertEquals(784, planalyzer.QueryPlan.CompileMemory);
			});
		}

		public void TestAnalyzeQueryPlan7()
		{
			var planalyzer = new QueryPlanalyzer(QueryPlanXml7);

			AssertEquals("SELECT [JH_PK] FROM [JobHeader] WHERE [JH_IsValid]=@1 ORDER BY [JH_JobPlannedStartDate] ASC", planalyzer.SqlStatement);
			AssertEquals(0, planalyzer.TableScans.Count());
			AssertEquals(0, planalyzer.IndexSeeks.Count());
			AssertEquals(1, planalyzer.IndexScans.Count());

			AssertEquals(2, planalyzer.Elements.Count);
			AssertEquals(1, planalyzer.Sorts.Count);
			AssertEquals(planalyzer.Elements[0], planalyzer.Sorts[0]);

			var sort = planalyzer.Sorts[0];
			AssertEquals("Sort", sort.PhysicalOperator);
			AssertEquals(0.000100027M, sort.EstimateCPU);
			AssertEquals(0.0112613M, sort.EstimateIO);
			AssertEquals(78, sort.EstimateCostPercent);
			AssertEquals(1, sort.SortColumns.Count);
			AssertEquals("JH_JobPlannedStartDate", sort.SortColumns[0].ColumnName);

			AssertEquals("Clustered Index Scan", planalyzer.Elements[1].PhysicalOperator);
			AssertEquals(0.0001581M, planalyzer.Elements[1].EstimateCPU);
			AssertEquals(0.003125M, planalyzer.Elements[1].EstimateIO);
			AssertEquals(22, planalyzer.Elements[1].EstimateCostPercent);
		}

		#region Query Plans XML

		const string QueryPlanXml1 =
@"<ShowPlanXML xmlns=""http://schemas.microsoft.com/sqlserver/2004/07/showplan"" Version=""1.5"" Build=""13.0.4457.0"">
	<BatchSequence>
		<Batch>
			<Statements>
				<StmtSimple StatementText=""SELECT COUNT(*) FROM dbo.GlbStaff WHERE GS_Code = &apos;DJT&apos;"" StatementId=""1"" StatementCompId=""1"" StatementType=""SELECT"" RetrievedFromCache=""true"" StatementSubTreeCost=""0.0032842"" StatementEstRows=""1"" SecurityPolicyApplied=""false"" StatementOptmLevel=""TRIVIAL"" QueryHash=""0xC9ED2307C34614B6"" QueryPlanHash=""0x68D694FA073C7099"" CardinalityEstimationModelVersion=""130"" ParameterizedText=""(@1 varchar(8000))SELECT COUNT(*) FROM [GlbStaff] WHERE [GS_Code]=@1"">
					<StatementSetOptions QUOTED_IDENTIFIER=""true"" ARITHABORT=""true"" CONCAT_NULL_YIELDS_NULL=""true"" ANSI_NULLS=""true"" ANSI_PADDING=""true"" ANSI_WARNINGS=""true"" NUMERIC_ROUNDABORT=""false"">
					</StatementSetOptions>
					<QueryPlan CachedPlanSize=""16"" CompileTime=""1"" CompileCPU=""1"" CompileMemory=""248"">
						<MemoryGrantInfo SerialRequiredMemory=""0"" SerialDesiredMemory=""0"">
						</MemoryGrantInfo>
						<OptimizerHardwareDependentProperties EstimatedAvailableMemoryGrant=""139460"" EstimatedPagesCached=""104595"" EstimatedAvailableDegreeOfParallelism=""6"" MaxCompileMemory=""11475856"">
						</OptimizerHardwareDependentProperties>
						<RelOp NodeId=""0"" PhysicalOp=""Compute Scalar"" LogicalOp=""Compute Scalar"" EstimateRows=""1"" EstimateIO=""0"" EstimateCPU=""0"" AvgRowSize=""11"" EstimatedTotalSubtreeCost=""0.0032842"" Parallel=""0"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimatedExecutionMode=""Row"">
							<OutputList>
								<ColumnReference Column=""Expr1002"">
								</ColumnReference>
							</OutputList>
							<ComputeScalar>
								<DefinedValues>
									<DefinedValue>
										<ColumnReference Column=""Expr1002"">
										</ColumnReference>
										<ScalarOperator ScalarString=""CONVERT_IMPLICIT(int,[Expr1003],0)"">
											<Convert DataType=""int"" Style=""0"" Implicit=""1"">
												<ScalarOperator>
													<Identifier>
														<ColumnReference Column=""Expr1003"">
														</ColumnReference>
													</Identifier>
												</ScalarOperator>
											</Convert>
										</ScalarOperator>
									</DefinedValue>
								</DefinedValues>
								<RelOp NodeId=""1"" PhysicalOp=""Stream Aggregate"" LogicalOp=""Aggregate"" EstimateRows=""1"" EstimateIO=""0"" EstimateCPU=""1.1e-006"" AvgRowSize=""11"" EstimatedTotalSubtreeCost=""0.0032842"" Parallel=""0"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimatedExecutionMode=""Row"">
									<OutputList>
										<ColumnReference Column=""Expr1003"">
										</ColumnReference>
									</OutputList>
									<StreamAggregate>
										<DefinedValues>
											<DefinedValue>
												<ColumnReference Column=""Expr1003"">
												</ColumnReference>
												<ScalarOperator ScalarString=""Count(*)"">
													<Aggregate AggType=""countstar"" Distinct=""0"">
													</Aggregate>
												</ScalarOperator>
											</DefinedValue>
										</DefinedValues>
										<RelOp NodeId=""2"" PhysicalOp=""Clustered Index Seek"" LogicalOp=""Clustered Index Seek"" EstimateRows=""1"" EstimatedRowsRead=""1"" EstimateIO=""0.003125"" EstimateCPU=""0.0001581"" AvgRowSize=""9"" EstimatedTotalSubtreeCost=""0.0032831"" TableCardinality=""7"" Parallel=""0"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimatedExecutionMode=""Row"">
											<OutputList>
											</OutputList>
											<IndexScan Ordered=""1"" ScanDirection=""FORWARD"" ForcedIndex=""0"" ForceSeek=""0"" ForceScan=""0"" NoExpandHint=""0"" Storage=""RowStore"">
												<DefinedValues>
												</DefinedValues>
												<Object Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[GlbStaff]"" Index=""[NR_UC__GS_Code]"" IndexKind=""Clustered"" Storage=""RowStore"">
												</Object>
												<SeekPredicates>
													<SeekPredicateNew>
														<SeekKeys>
														<Prefix ScanType=""EQ"">
															<RangeColumns>
																<ColumnReference Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[GlbStaff]"" Column=""GS_Code"">
																</ColumnReference>
															</RangeColumns>
															<RangeExpressions>
																<ScalarOperator ScalarString=""[@1]"">
																	<Identifier>
																		<ColumnReference Column=""@1"">
																		</ColumnReference>
																	</Identifier>
																</ScalarOperator>
															</RangeExpressions>
														</Prefix>
														</SeekKeys>
													</SeekPredicateNew>
												</SeekPredicates>
											</IndexScan>
										</RelOp>
									</StreamAggregate>
								</RelOp>
							</ComputeScalar>
						</RelOp>
						<ParameterList>
						<ColumnReference Column=""@1"" ParameterDataType=""varchar(8000)"" ParameterCompiledValue=""&apos;DJT&apos;"">
						</ColumnReference>
						</ParameterList>
					</QueryPlan>
				</StmtSimple>
			</Statements>
		</Batch>
	</BatchSequence>
</ShowPlanXML>
";

		const string QueryPlanXml2 =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<ShowPlanXML xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1.5"" Build=""13.0.4457.0"" xmlns=""http://schemas.microsoft.com/sqlserver/2004/07/showplan"">
  <BatchSequence>
    <Batch>
      <Statements>
        <StmtSimple StatementCompId=""1"" StatementEstRows=""1"" StatementId=""1"" StatementOptmLevel=""TRIVIAL"" CardinalityEstimationModelVersion=""130"" StatementSubTreeCost=""0.00330731"" StatementText=""SELECT COUNT(*) FROM dbo.ProcessTasks WHERE P9_GS_NKAssignedStaffMember = 'DJT'"" StatementType=""SELECT"" ParameterizedText=""(@1 varchar(8000))SELECT COUNT(*) FROM [ProcessTasks] WHERE [P9_GS_NKAssignedStaffMember]=@1"" QueryHash=""0xACE92650292FD76E"" QueryPlanHash=""0x71F74213E9F707A5"" RetrievedFromCache=""false"" SecurityPolicyApplied=""false"">
          <StatementSetOptions ANSI_NULLS=""true"" ANSI_PADDING=""true"" ANSI_WARNINGS=""true"" ARITHABORT=""true"" CONCAT_NULL_YIELDS_NULL=""true"" NUMERIC_ROUNDABORT=""false"" QUOTED_IDENTIFIER=""true"" />
          <QueryPlan CachedPlanSize=""16"" CompileTime=""13"" CompileCPU=""13"" CompileMemory=""544"">
            <MemoryGrantInfo SerialRequiredMemory=""0"" SerialDesiredMemory=""0"" />
            <OptimizerHardwareDependentProperties EstimatedAvailableMemoryGrant=""139460"" EstimatedPagesCached=""104595"" EstimatedAvailableDegreeOfParallelism=""6"" MaxCompileMemory=""11114712"" />
            <RelOp AvgRowSize=""11"" EstimateCPU=""0"" EstimateIO=""0"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimatedExecutionMode=""Row"" EstimateRows=""1"" LogicalOp=""Compute Scalar"" NodeId=""0"" Parallel=""false"" PhysicalOp=""Compute Scalar"" EstimatedTotalSubtreeCost=""0.00330731"">
              <OutputList>
                <ColumnReference Column=""Expr1003"" />
              </OutputList>
              <ComputeScalar>
                <DefinedValues>
                  <DefinedValue>
                    <ColumnReference Column=""Expr1003"" />
                    <ScalarOperator ScalarString=""CONVERT_IMPLICIT(int,[Expr1004],0)"">
                      <Convert DataType=""int"" Style=""0"" Implicit=""true"">
                        <ScalarOperator>
                          <Identifier>
                            <ColumnReference Column=""Expr1004"" />
                          </Identifier>
                        </ScalarOperator>
                      </Convert>
                    </ScalarOperator>
                  </DefinedValue>
                </DefinedValues>
                <RelOp AvgRowSize=""11"" EstimateCPU=""9.257E-06"" EstimateIO=""0"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimatedExecutionMode=""Row"" EstimateRows=""1"" LogicalOp=""Aggregate"" NodeId=""1"" Parallel=""false"" PhysicalOp=""Stream Aggregate"" EstimatedTotalSubtreeCost=""0.00330731"">
                  <OutputList>
                    <ColumnReference Column=""Expr1004"" />
                  </OutputList>
                  <StreamAggregate>
                    <DefinedValues>
                      <DefinedValue>
                        <ColumnReference Column=""Expr1004"" />
                        <ScalarOperator ScalarString=""Count(*)"">
                          <Aggregate AggType=""countstar"" Distinct=""false"" />
                        </ScalarOperator>
                      </DefinedValue>
                    </DefinedValues>
                    <RelOp AvgRowSize=""9"" EstimateCPU=""0.000173054"" EstimateIO=""0.003125"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimatedExecutionMode=""Row"" EstimateRows=""14.595"" EstimatedRowsRead=""14.595"" LogicalOp=""Index Seek"" NodeId=""2"" Parallel=""false"" PhysicalOp=""Index Seek"" EstimatedTotalSubtreeCost=""0.00329805"" TableCardinality=""303"">
                      <OutputList />
                      <IndexScan Ordered=""true"" ScanDirection=""FORWARD"" ForcedIndex=""false"" ForceSeek=""false"" ForceScan=""false"" NoExpandHint=""false"" Storage=""RowStore"">
                        <DefinedValues />
                        <Object Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[ProcessTasks]"" Index=""[NR_RX__P9_GS_NKAssignedStaffMember_P9_Status]"" IndexKind=""NonClustered"" Storage=""RowStore"" />
                        <SeekPredicates>
                          <SeekPredicateNew>
                            <SeekKeys>
                              <Prefix ScanType=""EQ"">
                                <RangeColumns>
                                  <ColumnReference Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[ProcessTasks]"" Column=""P9_GS_NKAssignedStaffMember"" />
                                </RangeColumns>
                                <RangeExpressions>
                                  <ScalarOperator ScalarString=""[@1]"">
                                    <Identifier>
                                      <ColumnReference Column=""@1"" />
                                    </Identifier>
                                  </ScalarOperator>
                                </RangeExpressions>
                              </Prefix>
                            </SeekKeys>
                          </SeekPredicateNew>
                        </SeekPredicates>
                      </IndexScan>
                    </RelOp>
                  </StreamAggregate>
                </RelOp>
              </ComputeScalar>
            </RelOp>
            <ParameterList>
              <ColumnReference Column=""@1"" ParameterDataType=""varchar(8000)"" ParameterCompiledValue=""'DJT'"" />
            </ParameterList>
          </QueryPlan>
        </StmtSimple>
      </Statements>
    </Batch>
  </BatchSequence>
</ShowPlanXML>";

		const string QueryPlanXml3 =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<ShowPlanXML xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1.5"" Build=""13.0.4457.0"" xmlns=""http://schemas.microsoft.com/sqlserver/2004/07/showplan"">
  <BatchSequence>
    <Batch>
      <Statements>
        <StmtSimple StatementCompId=""1"" StatementEstRows=""1"" StatementId=""1"" StatementOptmLevel=""TRIVIAL"" CardinalityEstimationModelVersion=""130"" StatementSubTreeCost=""0.0032832"" StatementText=""select top 1 GS_Code from dbo.GlbStaff"" StatementType=""SELECT"" QueryHash=""0xA838B84EFA7639CD"" QueryPlanHash=""0x409BFC1810F7D241"" RetrievedFromCache=""false"" SecurityPolicyApplied=""false"">
          <StatementSetOptions ANSI_NULLS=""true"" ANSI_PADDING=""true"" ANSI_WARNINGS=""true"" ARITHABORT=""true"" CONCAT_NULL_YIELDS_NULL=""true"" NUMERIC_ROUNDABORT=""false"" QUOTED_IDENTIFIER=""true"" />
          <QueryPlan CachedPlanSize=""16"" CompileTime=""0"" CompileCPU=""0"" CompileMemory=""216"">
            <MemoryGrantInfo SerialRequiredMemory=""0"" SerialDesiredMemory=""0"" />
            <OptimizerHardwareDependentProperties EstimatedAvailableMemoryGrant=""139460"" EstimatedPagesCached=""104595"" EstimatedAvailableDegreeOfParallelism=""6"" MaxCompileMemory=""11120280"" />
            <RelOp AvgRowSize=""12"" EstimateCPU=""1E-07"" EstimateIO=""0"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimatedExecutionMode=""Row"" EstimateRows=""1"" LogicalOp=""Top"" NodeId=""0"" Parallel=""false"" PhysicalOp=""Top"" EstimatedTotalSubtreeCost=""0.0032832"">
              <OutputList>
                <ColumnReference Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[GlbStaff]"" Column=""GS_Code"" />
              </OutputList>
              <Top RowCount=""false"" IsPercent=""false"" WithTies=""false"">
                <TopExpression>
                  <ScalarOperator ScalarString=""(1)"">
                    <Const ConstValue=""(1)"" />
                  </ScalarOperator>
                </TopExpression>
                <RelOp AvgRowSize=""12"" EstimateCPU=""0.0001647"" EstimateIO=""0.003125"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimatedExecutionMode=""Row"" EstimateRows=""1"" EstimatedRowsRead=""7"" LogicalOp=""Index Scan"" NodeId=""1"" Parallel=""false"" PhysicalOp=""Index Scan"" EstimatedTotalSubtreeCost=""0.0032831"" TableCardinality=""7"">
                  <OutputList>
                    <ColumnReference Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[GlbStaff]"" Column=""GS_Code"" />
                  </OutputList>
                  <IndexScan Ordered=""false"" ForcedIndex=""false"" ForceSeek=""false"" ForceScan=""false"" NoExpandHint=""false"" Storage=""RowStore"">
                    <DefinedValues>
                      <DefinedValue>
                        <ColumnReference Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[GlbStaff]"" Column=""GS_Code"" />
                      </DefinedValue>
                    </DefinedValues>
                    <Object Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[GlbStaff]"" Index=""[FK_RX__GS_PER]"" IndexKind=""NonClustered"" Storage=""RowStore"" />
                  </IndexScan>
                </RelOp>
              </Top>
            </RelOp>
          </QueryPlan>
        </StmtSimple>
      </Statements>
    </Batch>
  </BatchSequence>
</ShowPlanXML>";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1072:DoNotUseDBNameInSQLCommandsAnalyzer", Justification = "Part of a query plan. DB name must be included.")]
		const string QueryPlanXml4 =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<ShowPlanXML xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1.5"" Build=""13.0.4457.0"" xmlns=""http://schemas.microsoft.com/sqlserver/2004/07/showplan"">
  <BatchSequence>
    <Batch>
      <Statements>
        <StmtSimple StatementCompId=""1"" StatementEstRows=""1"" StatementId=""1"" StatementOptmLevel=""FULL"" StatementOptmEarlyAbortReason=""GoodEnoughPlanFound"" CardinalityEstimationModelVersion=""130"" StatementSubTreeCost=""0.0462477"" StatementText=""SELECT COUNT(*) FROM dbo.ProcessHeader WHERE FH_CompletionStatement = 'Donald J. Trump'"" StatementType=""SELECT"" ParameterizedText=""(@1 varchar(8000))SELECT COUNT(*) FROM [ProcessHeader] WHERE [FH_CompletionStatement]=@1"" QueryHash=""0xD91BDF760A28E4E0"" QueryPlanHash=""0x7C5CCB57CC718D08"" RetrievedFromCache=""false"" SecurityPolicyApplied=""false"">
          <StatementSetOptions ANSI_NULLS=""true"" ANSI_PADDING=""true"" ANSI_WARNINGS=""true"" ARITHABORT=""true"" CONCAT_NULL_YIELDS_NULL=""true"" NUMERIC_ROUNDABORT=""false"" QUOTED_IDENTIFIER=""true"" />
          <QueryPlan CachedPlanSize=""16"" CompileTime=""3"" CompileCPU=""3"" CompileMemory=""320"">
            <MemoryGrantInfo SerialRequiredMemory=""0"" SerialDesiredMemory=""0"" />
            <OptimizerHardwareDependentProperties EstimatedAvailableMemoryGrant=""139460"" EstimatedPagesCached=""104595"" EstimatedAvailableDegreeOfParallelism=""6"" MaxCompileMemory=""11119992"" />
            <RelOp AvgRowSize=""11"" EstimateCPU=""0"" EstimateIO=""0"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimatedExecutionMode=""Row"" EstimateRows=""1"" LogicalOp=""Compute Scalar"" NodeId=""0"" Parallel=""false"" PhysicalOp=""Compute Scalar"" EstimatedTotalSubtreeCost=""0.0462477"">
              <OutputList>
                <ColumnReference Column=""Expr1003"" />
              </OutputList>
              <ComputeScalar>
                <DefinedValues>
                  <DefinedValue>
                    <ColumnReference Column=""Expr1003"" />
                    <ScalarOperator ScalarString=""CONVERT_IMPLICIT(int,[Expr1007],0)"">
                      <Convert DataType=""int"" Style=""0"" Implicit=""true"">
                        <ScalarOperator>
                          <Identifier>
                            <ColumnReference Column=""Expr1007"" />
                          </Identifier>
                        </ScalarOperator>
                      </Convert>
                    </ScalarOperator>
                  </DefinedValue>
                </DefinedValues>
                <RelOp AvgRowSize=""11"" EstimateCPU=""1.1E-06"" EstimateIO=""0"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimatedExecutionMode=""Row"" EstimateRows=""1"" LogicalOp=""Aggregate"" NodeId=""1"" Parallel=""false"" PhysicalOp=""Stream Aggregate"" EstimatedTotalSubtreeCost=""0.0462477"">
                  <OutputList>
                    <ColumnReference Column=""Expr1007"" />
                  </OutputList>
                  <StreamAggregate>
                    <DefinedValues>
                      <DefinedValue>
                        <ColumnReference Column=""Expr1007"" />
                        <ScalarOperator ScalarString=""Count(*)"">
                          <Aggregate AggType=""countstar"" Distinct=""false"" />
                        </ScalarOperator>
                      </DefinedValue>
                    </DefinedValues>
                    <RelOp AvgRowSize=""9"" EstimateCPU=""5.8E-07"" EstimateIO=""0"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimatedExecutionMode=""Row"" EstimateRows=""1"" LogicalOp=""Filter"" NodeId=""2"" Parallel=""false"" PhysicalOp=""Filter"" EstimatedTotalSubtreeCost=""0.0462466"">
                      <OutputList />
                      <Filter StartupExpression=""false"">
                        <RelOp AvgRowSize=""4035"" EstimateCPU=""0.0001581"" EstimateIO=""0.046088"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimatedExecutionMode=""Row"" EstimateRows=""1"" EstimatedRowsRead=""1"" LogicalOp=""Table Scan"" NodeId=""3"" Parallel=""false"" PhysicalOp=""Table Scan"" EstimatedTotalSubtreeCost=""0.0462461"" TableCardinality=""0"">
                          <OutputList>
                            <ColumnReference Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[ProcessHeader]"" Column=""FH_CompletionStatement"" />
                          </OutputList>
                          <TableScan Ordered=""false"" ForcedIndex=""false"" ForceScan=""false"" NoExpandHint=""false"" Storage=""RowStore"">
                            <DefinedValues>
                              <DefinedValue>
                                <ColumnReference Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[ProcessHeader]"" Column=""FH_CompletionStatement"" />
                              </DefinedValue>
                            </DefinedValues>
                            <Object Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[ProcessHeader]"" IndexKind=""Heap"" Storage=""RowStore"" />
                          </TableScan>
                        </RelOp>
                        <Predicate>
                          <ScalarOperator ScalarString=""[Odyssey].[dbo].[ProcessHeader].[FH_CompletionStatement]=CONVERT_IMPLICIT(nvarchar(max),'Donald J. Trump',0)"">
                            <Compare CompareOp=""EQ"">
                              <ScalarOperator>
                                <Identifier>
                                  <ColumnReference Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[ProcessHeader]"" Column=""FH_CompletionStatement"" />
                                </Identifier>
                              </ScalarOperator>
                              <ScalarOperator>
                                <Convert DataType=""nvarchar(max)"" Length=""2147483647"" Style=""0"" Implicit=""true"">
                                  <ScalarOperator>
                                    <Const ConstValue=""'Donald J. Trump'"" />
                                  </ScalarOperator>
                                </Convert>
                              </ScalarOperator>
                            </Compare>
                          </ScalarOperator>
                        </Predicate>
                      </Filter>
                    </RelOp>
                  </StreamAggregate>
                </RelOp>
              </ComputeScalar>
            </RelOp>
            <ParameterList>
              <ColumnReference Column=""@1"" ParameterDataType=""varchar(8000)"" ParameterCompiledValue=""'Donald J. Trump'"" />
            </ParameterList>
          </QueryPlan>
        </StmtSimple>
      </Statements>
    </Batch>
  </BatchSequence>
</ShowPlanXML>";

		const string QueryPlanXml5 =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<ShowPlanXML xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1.5"" Build=""13.0.4457.0"" xmlns=""http://schemas.microsoft.com/sqlserver/2004/07/showplan"">
  <BatchSequence>
    <Batch>
      <Statements>
        <StmtSimple StatementCompId=""1"" StatementEstRows=""1"" StatementId=""1"" StatementOptmLevel=""FULL"" StatementOptmEarlyAbortReason=""GoodEnoughPlanFound"" CardinalityEstimationModelVersion=""130"" StatementSubTreeCost=""0.0221259"" StatementText=""SELECT [P9_Type],[P9_Description] FROM [ProcessTasks] WHERE [P9_GS_NKAssignedStaffMember]=@1"" StatementType=""SELECT"" QueryHash=""0x483D75D2188D444D"" QueryPlanHash=""0x7B64F5394CA2090D"" RetrievedFromCache=""false"" SecurityPolicyApplied=""false"">
          <StatementSetOptions ANSI_NULLS=""true"" ANSI_PADDING=""true"" ANSI_WARNINGS=""true"" ARITHABORT=""true"" CONCAT_NULL_YIELDS_NULL=""true"" NUMERIC_ROUNDABORT=""false"" QUOTED_IDENTIFIER=""true"" />
          <QueryPlan DegreeOfParallelism=""1"" CachedPlanSize=""24"" CompileTime=""2"" CompileCPU=""2"" CompileMemory=""520"">
            <MemoryGrantInfo SerialRequiredMemory=""0"" SerialDesiredMemory=""0"" />
            <OptimizerHardwareDependentProperties EstimatedAvailableMemoryGrant=""139460"" EstimatedPagesCached=""104595"" EstimatedAvailableDegreeOfParallelism=""6"" MaxCompileMemory=""10948712"" />
            <QueryTimeStats CpuTime=""0"" ElapsedTime=""0"" />
            <RelOp AvgRowSize=""64"" EstimateCPU=""4.18E-06"" EstimateIO=""0"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimatedExecutionMode=""Row"" EstimateRows=""1"" LogicalOp=""Inner Join"" NodeId=""0"" Parallel=""false"" PhysicalOp=""Nested Loops"" EstimatedTotalSubtreeCost=""0.0221259"">
              <OutputList>
                <ColumnReference Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[ProcessTasks]"" Column=""P9_Type"" />
                <ColumnReference Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[ProcessTasks]"" Column=""P9_Description"" />
              </OutputList>
              <RunTimeInformation>
                <RunTimeCountersPerThread Thread=""0"" ActualRows=""0"" Batches=""0"" ActualEndOfScans=""1"" ActualExecutions=""1"" ActualExecutionMode=""Row"" ActualElapsedms=""0"" ActualCPUms=""0"" />
              </RunTimeInformation>
              <NestedLoops Optimized=""false"">
                <OuterReferences>
                  <ColumnReference Column=""Bmk1000"" />
                </OuterReferences>
                <RelOp AvgRowSize=""15"" EstimateCPU=""0.0001581"" EstimateIO=""0.003125"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimatedExecutionMode=""Row"" EstimateRows=""1"" EstimatedRowsRead=""1"" LogicalOp=""Index Seek"" NodeId=""1"" Parallel=""false"" PhysicalOp=""Index Seek"" EstimatedTotalSubtreeCost=""0.0032831"" TableCardinality=""303"">
                  <OutputList>
                    <ColumnReference Column=""Bmk1000"" />
                  </OutputList>
                  <RunTimeInformation>
                    <RunTimeCountersPerThread Thread=""0"" ActualRows=""0"" Batches=""0"" ActualEndOfScans=""1"" ActualExecutions=""1"" ActualExecutionMode=""Row"" ActualElapsedms=""0"" ActualCPUms=""0"" ActualScans=""1"" ActualLogicalReads=""3"" ActualPhysicalReads=""0"" ActualReadAheads=""0"" ActualLobLogicalReads=""0"" ActualLobPhysicalReads=""0"" ActualLobReadAheads=""0"" />
                  </RunTimeInformation>
                  <IndexScan Ordered=""true"" ScanDirection=""FORWARD"" ForcedIndex=""false"" ForceSeek=""false"" ForceScan=""false"" NoExpandHint=""false"" Storage=""RowStore"">
                    <DefinedValues>
                      <DefinedValue>
                        <ColumnReference Column=""Bmk1000"" />
                      </DefinedValue>
                    </DefinedValues>
                    <Object Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[ProcessTasks]"" Index=""[NR_RX__P9_GS_NKAssignedStaffMember_P9_Status]"" IndexKind=""NonClustered"" Storage=""RowStore"" />
                    <SeekPredicates>
                      <SeekPredicateNew>
                        <SeekKeys>
                          <Prefix ScanType=""EQ"">
                            <RangeColumns>
                              <ColumnReference Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[ProcessTasks]"" Column=""P9_GS_NKAssignedStaffMember"" />
                            </RangeColumns>
                            <RangeExpressions>
                              <ScalarOperator ScalarString=""'DJT'"">
                                <Const ConstValue=""'DJT'"" />
                              </ScalarOperator>
                            </RangeExpressions>
                          </Prefix>
                        </SeekKeys>
                      </SeekPredicateNew>
                    </SeekPredicates>
                  </IndexScan>
                </RelOp>
                <RelOp AvgRowSize=""64"" EstimateCPU=""0.0001581"" EstimateIO=""0.0186806"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimatedExecutionMode=""Row"" EstimateRows=""1"" LogicalOp=""RID Lookup"" NodeId=""3"" Parallel=""false"" PhysicalOp=""RID Lookup"" EstimatedTotalSubtreeCost=""0.0188387"" TableCardinality=""303"">
                  <OutputList>
                    <ColumnReference Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[ProcessTasks]"" Column=""P9_Type"" />
                    <ColumnReference Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[ProcessTasks]"" Column=""P9_Description"" />
                  </OutputList>
                  <RunTimeInformation>
                    <RunTimeCountersPerThread Thread=""0"" ActualRows=""0"" Batches=""0"" ActualEndOfScans=""0"" ActualExecutions=""0"" ActualExecutionMode=""Row"" ActualElapsedms=""0"" ActualCPUms=""0"" ActualScans=""0"" ActualLogicalReads=""0"" ActualPhysicalReads=""0"" ActualReadAheads=""0"" ActualLobLogicalReads=""0"" ActualLobPhysicalReads=""0"" ActualLobReadAheads=""0"" />
                  </RunTimeInformation>
                  <IndexScan Lookup=""true"" Ordered=""true"" ScanDirection=""FORWARD"" ForcedIndex=""false"" ForceSeek=""false"" ForceScan=""false"" NoExpandHint=""false"" Storage=""RowStore"">
                    <DefinedValues>
                      <DefinedValue>
                        <ColumnReference Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[ProcessTasks]"" Column=""P9_Type"" />
                      </DefinedValue>
                      <DefinedValue>
                        <ColumnReference Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[ProcessTasks]"" Column=""P9_Description"" />
                      </DefinedValue>
                    </DefinedValues>
                    <Object Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[ProcessTasks]"" TableReferenceId=""-1"" IndexKind=""Heap"" Storage=""RowStore"" />
                    <SeekPredicates>
                      <SeekPredicateNew>
                        <SeekKeys>
                          <Prefix ScanType=""EQ"">
                            <RangeColumns>
                              <ColumnReference Column=""Bmk1000"" />
                            </RangeColumns>
                            <RangeExpressions>
                              <ScalarOperator ScalarString=""[Bmk1000]"">
                                <Identifier>
                                  <ColumnReference Column=""Bmk1000"" />
                                </Identifier>
                              </ScalarOperator>
                            </RangeExpressions>
                          </Prefix>
                        </SeekKeys>
                      </SeekPredicateNew>
                    </SeekPredicates>
                  </IndexScan>
                </RelOp>
              </NestedLoops>
            </RelOp>
            <ParameterList>
              <ColumnReference Column=""@1"" ParameterDataType=""varchar(8000)"" ParameterCompiledValue=""'DJT'"" ParameterRuntimeValue=""'DJT'"" />
            </ParameterList>
          </QueryPlan>
        </StmtSimple>
      </Statements>
    </Batch>
  </BatchSequence>
</ShowPlanXML>";

		const string QueryPlanXml6 = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ShowPlanXML xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1.539"" Build=""15.0.4236.7"" xmlns=""http://schemas.microsoft.com/sqlserver/2004/07/showplan"">
  <BatchSequence>
    <Batch>
      <Statements>
        <StmtSimple StatementCompId=""1"" StatementEstRows=""1"" StatementId=""1"" StatementOptmLevel=""TRIVIAL"" CardinalityEstimationModelVersion=""150"" StatementSubTreeCost=""0.0032859"" StatementText=""SELECT COUNT(*) FROM dbo.AccTransactionHeader WHERE AH_ConsolidatedInvoiceRef IN ('A', 'B', 'C', 'S00001561', 'S00001413')"" StatementType=""SELECT"" QueryHash=""0xD78DE28C034485CC"" QueryPlanHash=""0x254CDD28C53389C7"" RetrievedFromCache=""true"" SecurityPolicyApplied=""false"">
          <StatementSetOptions ANSI_NULLS=""true"" ANSI_PADDING=""true"" ANSI_WARNINGS=""true"" ARITHABORT=""true"" CONCAT_NULL_YIELDS_NULL=""true"" NUMERIC_ROUNDABORT=""false"" QUOTED_IDENTIFIER=""true"" />
          <QueryPlan DegreeOfParallelism=""1"" CachedPlanSize=""24"" CompileTime=""2"" CompileCPU=""2"" CompileMemory=""784"">
            <MemoryGrantInfo SerialRequiredMemory=""0"" SerialDesiredMemory=""0"" GrantedMemory=""0"" MaxUsedMemory=""0"" />
            <OptimizerHardwareDependentProperties EstimatedAvailableMemoryGrant=""119837"" EstimatedPagesCached=""209715"" EstimatedAvailableDegreeOfParallelism=""14"" MaxCompileMemory=""19163800"" />
            <OptimizerStatsUsage>
              <StatisticsInfo Database=""[OdysseyTrainingModel]"" Schema=""[dbo]"" Table=""[AccTransactionHeader]"" Statistics=""[NR_RX__AH_ConsolidatedInvoiceRef]"" ModificationCount=""22"" SamplingPercent=""100"" LastUpdate=""2022-07-29T11:28:06.27"" />
            </OptimizerStatsUsage>
            <QueryTimeStats CpuTime=""0"" ElapsedTime=""0"" />
            <RelOp AvgRowSize=""11"" EstimateCPU=""0"" EstimateIO=""0"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimatedExecutionMode=""Row"" EstimateRows=""1"" LogicalOp=""Compute Scalar"" NodeId=""0"" Parallel=""false"" PhysicalOp=""Compute Scalar"" EstimatedTotalSubtreeCost=""0.0032859"">
              <OutputList>
                <ColumnReference Column=""Expr1002"" />
              </OutputList>
              <ComputeScalar>
                <DefinedValues>
                  <DefinedValue>
                    <ColumnReference Column=""Expr1002"" />
                    <ScalarOperator ScalarString=""CONVERT_IMPLICIT(int,[Expr1003],0)"">
                      <Convert DataType=""int"" Style=""0"" Implicit=""true"">
                        <ScalarOperator>
                          <Identifier>
                            <ColumnReference Column=""Expr1003"" />
                          </Identifier>
                        </ScalarOperator>
                      </Convert>
                    </ScalarOperator>
                  </DefinedValue>
                </DefinedValues>
                <RelOp AvgRowSize=""11"" EstimateCPU=""1.7E-06"" EstimateIO=""0"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimatedExecutionMode=""Row"" EstimateRows=""1"" LogicalOp=""Aggregate"" NodeId=""1"" Parallel=""false"" PhysicalOp=""Stream Aggregate"" EstimatedTotalSubtreeCost=""0.0032859"">
                  <OutputList>
                    <ColumnReference Column=""Expr1003"" />
                  </OutputList>
                  <RunTimeInformation>
                    <RunTimeCountersPerThread Thread=""0"" ActualRows=""1"" Batches=""0"" ActualEndOfScans=""1"" ActualExecutions=""1"" ActualExecutionMode=""Row"" ActualElapsedms=""0"" ActualCPUms=""0"" />
                  </RunTimeInformation>
                  <StreamAggregate>
                    <DefinedValues>
                      <DefinedValue>
                        <ColumnReference Column=""Expr1003"" />
                        <ScalarOperator ScalarString=""Count(*)"">
                          <Aggregate AggType=""countstar"" Distinct=""false"" />
                        </ScalarOperator>
                      </DefinedValue>
                    </DefinedValues>
                    <RelOp AvgRowSize=""9"" EstimateCPU=""0.0001592"" EstimateIO=""0.003125"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimatedExecutionMode=""Row"" EstimateRows=""2"" EstimatedRowsRead=""2"" LogicalOp=""Index Seek"" NodeId=""2"" Parallel=""false"" PhysicalOp=""Index Seek"" EstimatedTotalSubtreeCost=""0.0032842"" TableCardinality=""252"">
                      <OutputList />
                      <RunTimeInformation>
                        <RunTimeCountersPerThread Thread=""0"" ActualRows=""2"" ActualRowsRead=""2"" Batches=""0"" ActualEndOfScans=""1"" ActualExecutions=""1"" ActualExecutionMode=""Row"" ActualElapsedms=""0"" ActualCPUms=""0"" ActualScans=""5"" ActualLogicalReads=""10"" ActualPhysicalReads=""0"" ActualReadAheads=""0"" ActualLobLogicalReads=""0"" ActualLobPhysicalReads=""0"" ActualLobReadAheads=""0"" />
                        <!-- Note the following element was NOT part of the original plan, but included to test aggregation of multiple threads -->
                        <RunTimeCountersPerThread Thread=""1"" ActualRows=""3"" ActualRowsRead=""9"" Batches=""0"" ActualEndOfScans=""2"" ActualExecutions=""2"" ActualExecutionMode=""Row"" ActualElapsedms=""0"" ActualCPUms=""0"" ActualScans=""15"" ActualLogicalReads=""4"" ActualPhysicalReads=""0"" ActualReadAheads=""0"" ActualLobLogicalReads=""0"" ActualLobPhysicalReads=""0"" ActualLobReadAheads=""0"" />
                      </RunTimeInformation>
                      <IndexScan Ordered=""true"" ScanDirection=""FORWARD"" ForcedIndex=""false"" ForceSeek=""false"" ForceScan=""false"" NoExpandHint=""false"" Storage=""RowStore"">
                        <DefinedValues />
                        <Object Database=""[OdysseyTrainingModel]"" Schema=""[dbo]"" Table=""[AccTransactionHeader]"" Index=""[NR_RX__AH_ConsolidatedInvoiceRef]"" IndexKind=""NonClustered"" Storage=""RowStore"" />
                        <SeekPredicates>
                          <SeekPredicateNew>
                            <SeekKeys>
                              <Prefix ScanType=""EQ"">
                                <RangeColumns>
                                  <ColumnReference Database=""[OdysseyTrainingModel]"" Schema=""[dbo]"" Table=""[AccTransactionHeader]"" Column=""AH_ConsolidatedInvoiceRef"" />
                                </RangeColumns>
                                <RangeExpressions>
                                  <ScalarOperator ScalarString=""'A'"">
                                    <Const ConstValue=""'A'"" />
                                  </ScalarOperator>
                                </RangeExpressions>
                              </Prefix>
                            </SeekKeys>
                          </SeekPredicateNew>
                          <SeekPredicateNew>
                            <SeekKeys>
                              <Prefix ScanType=""EQ"">
                                <RangeColumns>
                                  <ColumnReference Database=""[OdysseyTrainingModel]"" Schema=""[dbo]"" Table=""[AccTransactionHeader]"" Column=""AH_ConsolidatedInvoiceRef"" />
                                </RangeColumns>
                                <RangeExpressions>
                                  <ScalarOperator ScalarString=""'B'"">
                                    <Const ConstValue=""'B'"" />
                                  </ScalarOperator>
                                </RangeExpressions>
                              </Prefix>
                            </SeekKeys>
                          </SeekPredicateNew>
                          <SeekPredicateNew>
                            <SeekKeys>
                              <Prefix ScanType=""EQ"">
                                <RangeColumns>
                                  <ColumnReference Database=""[OdysseyTrainingModel]"" Schema=""[dbo]"" Table=""[AccTransactionHeader]"" Column=""AH_ConsolidatedInvoiceRef"" />
                                </RangeColumns>
                                <RangeExpressions>
                                  <ScalarOperator ScalarString=""'C'"">
                                    <Const ConstValue=""'C'"" />
                                  </ScalarOperator>
                                </RangeExpressions>
                              </Prefix>
                            </SeekKeys>
                          </SeekPredicateNew>
                          <SeekPredicateNew>
                            <SeekKeys>
                              <Prefix ScanType=""EQ"">
                                <RangeColumns>
                                  <ColumnReference Database=""[OdysseyTrainingModel]"" Schema=""[dbo]"" Table=""[AccTransactionHeader]"" Column=""AH_ConsolidatedInvoiceRef"" />
                                </RangeColumns>
                                <RangeExpressions>
                                  <ScalarOperator ScalarString=""'S00001413'"">
                                    <Const ConstValue=""'S00001413'"" />
                                  </ScalarOperator>
                                </RangeExpressions>
                              </Prefix>
                            </SeekKeys>
                          </SeekPredicateNew>
                          <SeekPredicateNew>
                            <SeekKeys>
                              <Prefix ScanType=""EQ"">
                                <RangeColumns>
                                  <ColumnReference Database=""[OdysseyTrainingModel]"" Schema=""[dbo]"" Table=""[AccTransactionHeader]"" Column=""AH_ConsolidatedInvoiceRef"" />
                                </RangeColumns>
                                <RangeExpressions>
                                  <ScalarOperator ScalarString=""'S00001561'"">
                                    <Const ConstValue=""'S00001561'"" />
                                  </ScalarOperator>
                                </RangeExpressions>
                              </Prefix>
                            </SeekKeys>
                          </SeekPredicateNew>
                        </SeekPredicates>
                      </IndexScan>
                    </RelOp>
                  </StreamAggregate>
                </RelOp>
              </ComputeScalar>
            </RelOp>
          </QueryPlan>
        </StmtSimple>
      </Statements>
    </Batch>
  </BatchSequence>
</ShowPlanXML>";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1072:DoNotUseDBNameInSQLCommandsAnalyzer", Justification = "Part of a query plan. DB name must be included.")]
		const string QueryPlanXml7 = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ShowPlanXML xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" Version=""1.539"" Build=""15.0.4326.1"" xmlns=""http://schemas.microsoft.com/sqlserver/2004/07/showplan"">
  <BatchSequence>
    <Batch>
      <Statements>
        <StmtSimple StatementCompId=""1"" StatementEstRows=""1"" StatementId=""1"" StatementOptmLevel=""FULL"" StatementOptmEarlyAbortReason=""GoodEnoughPlanFound"" CardinalityEstimationModelVersion=""150"" StatementSubTreeCost=""0.0146449"" StatementText=""SELECT [JH_PK] FROM [JobHeader] WHERE [JH_IsValid]=@1 ORDER BY [JH_JobPlannedStartDate] ASC"" StatementType=""SELECT"" QueryHash=""0xD10B42D025908800"" QueryPlanHash=""0x52297103B31340E7"" RetrievedFromCache=""false"" SecurityPolicyApplied=""false"">
          <StatementSetOptions ANSI_NULLS=""true"" ANSI_PADDING=""true"" ANSI_WARNINGS=""true"" ARITHABORT=""true"" CONCAT_NULL_YIELDS_NULL=""true"" NUMERIC_ROUNDABORT=""false"" QUOTED_IDENTIFIER=""true"" />
          <QueryPlan DegreeOfParallelism=""1"" MemoryGrant=""1024"" CachedPlanSize=""24"" CompileTime=""2"" CompileCPU=""2"" CompileMemory=""376"">
            <MemoryGrantInfo SerialRequiredMemory=""512"" SerialDesiredMemory=""544"" RequiredMemory=""512"" DesiredMemory=""544"" RequestedMemory=""1024"" GrantWaitTime=""0"" GrantedMemory=""1024"" MaxUsedMemory=""0"" MaxQueryMemory=""7238320"" LastRequestedMemory=""0"" IsMemoryGrantFeedbackAdjusted=""No: First Execution"" />
            <OptimizerHardwareDependentProperties EstimatedAvailableMemoryGrant=""333860"" EstimatedPagesCached=""417325"" EstimatedAvailableDegreeOfParallelism=""8"" MaxCompileMemory=""25437888"" />
            <QueryTimeStats CpuTime=""0"" ElapsedTime=""0"" />
            <RelOp AvgRowSize=""27"" EstimateCPU=""0.000100027"" EstimateIO=""0.0112613"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimatedExecutionMode=""Row"" EstimateRows=""1"" LogicalOp=""Sort"" NodeId=""0"" Parallel=""false"" PhysicalOp=""Sort"" EstimatedTotalSubtreeCost=""0.0146449"">
              <OutputList>
                <ColumnReference Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[JobHeader]"" Column=""JH_PK"" />
                <ColumnReference Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[JobHeader]"" Column=""JH_JobPlannedStartDate"" />
              </OutputList>
              <MemoryFractions Input=""1"" Output=""1"" />
              <RunTimeInformation>
                <RunTimeCountersPerThread Thread=""0"" ActualRebinds=""1"" ActualRewinds=""0"" ActualRows=""0"" Batches=""0"" ActualEndOfScans=""1"" ActualExecutions=""1"" ActualExecutionMode=""Row"" ActualElapsedms=""0"" ActualCPUms=""0"" ActualScans=""0"" ActualLogicalReads=""0"" ActualPhysicalReads=""0"" ActualReadAheads=""0"" ActualLobLogicalReads=""0"" ActualLobPhysicalReads=""0"" ActualLobReadAheads=""0"" InputMemoryGrant=""1024"" OutputMemoryGrant=""640"" UsedMemoryGrant=""0"" />
              </RunTimeInformation>
              <Sort Distinct=""false"">
                <OrderBy>
                  <OrderByColumn Ascending=""true"">
                    <ColumnReference Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[JobHeader]"" Column=""JH_JobPlannedStartDate"" />
                  </OrderByColumn>
                </OrderBy>
                <RelOp AvgRowSize=""28"" EstimateCPU=""0.0001581"" EstimateIO=""0.003125"" EstimateRebinds=""0"" EstimateRewinds=""0"" EstimatedExecutionMode=""Row"" EstimateRows=""1"" EstimatedRowsRead=""1"" LogicalOp=""Clustered Index Scan"" NodeId=""1"" Parallel=""false"" PhysicalOp=""Clustered Index Scan"" EstimatedTotalSubtreeCost=""0.0032831"" TableCardinality=""0"">
                  <OutputList>
                    <ColumnReference Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[JobHeader]"" Column=""JH_PK"" />
                    <ColumnReference Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[JobHeader]"" Column=""JH_JobPlannedStartDate"" />
                  </OutputList>
                  <RunTimeInformation>
                    <RunTimeCountersPerThread Thread=""0"" ActualRows=""0"" Batches=""0"" ActualEndOfScans=""1"" ActualExecutions=""1"" ActualExecutionMode=""Row"" ActualElapsedms=""0"" ActualCPUms=""0"" ActualScans=""1"" ActualLogicalReads=""2"" ActualPhysicalReads=""0"" ActualReadAheads=""0"" ActualLobLogicalReads=""0"" ActualLobPhysicalReads=""0"" ActualLobReadAheads=""0"" />
                  </RunTimeInformation>
                  <IndexScan Ordered=""false"" ForcedIndex=""false"" ForceScan=""false"" NoExpandHint=""false"" Storage=""RowStore"">
                    <DefinedValues>
                      <DefinedValue>
                        <ColumnReference Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[JobHeader]"" Column=""JH_PK"" />
                      </DefinedValue>
                      <DefinedValue>
                        <ColumnReference Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[JobHeader]"" Column=""JH_JobPlannedStartDate"" />
                      </DefinedValue>
                    </DefinedValues>
                    <Object Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[JobHeader]"" Index=""[FK_UC__JH_GC_JH_ParentID]"" IndexKind=""Clustered"" Storage=""RowStore"" />
                    <Predicate>
                      <ScalarOperator ScalarString=""[Odyssey].[dbo].[JobHeader].[JH_IsValid]=(1)"">
                        <Compare CompareOp=""EQ"">
                          <ScalarOperator>
                            <Identifier>
                              <ColumnReference Database=""[Odyssey]"" Schema=""[dbo]"" Table=""[JobHeader]"" Column=""JH_IsValid"" />
                            </Identifier>
                          </ScalarOperator>
                          <ScalarOperator>
                            <Const ConstValue=""(1)"" />
                          </ScalarOperator>
                        </Compare>
                      </ScalarOperator>
                    </Predicate>
                  </IndexScan>
                </RelOp>
              </Sort>
            </RelOp>
            <ParameterList>
              <ColumnReference Column=""@1"" ParameterDataType=""tinyint"" ParameterCompiledValue=""(1)"" ParameterRuntimeValue=""(1)"" />
            </ParameterList>
          </QueryPlan>
        </StmtSimple>
      </Statements>
    </Batch>
  </BatchSequence>
</ShowPlanXML>";

		#endregion
	}
}
