<?xml version="1.0" encoding="UTF-8"?>
<structure version="2" schemafile="ExportMessages.xsd" workingxmlfile="consolTest.xml" templatexmlfile="">
	<nspair prefix="xsi" uri="http://www.w3.org/2001/XMLSchema-instance"/>
	<template>
		<match overwrittenxslmatch="/"/>
		<children>
			<template>
				<match match="Consol"/>
				<children>
					<table dynamic="1" topdown="0">
						<properties border="1"/>
						<children>
							<tablebody>
								<children>
									<tablerow>
										<children>
											<tablecol>
												<children>
													<text fixtext="Identifier"/>
												</children>
											</tablecol>
											<tablecol dynamic="1">
												<children>
													<template>
														<match match="ConsolIdentifier"/>
														<children>
															<template>
																<match match="@ConsolIdentifierType"/>
																<children>
																	<select ownvalue="1" enumeration="1">
																		<properties size="0"/>
																	</select>
																</children>
															</template>
														</children>
													</template>
													<template>
														<match match="ConsolIdentifier"/>
														<children>
															<xpath allchildren="1"/>
														</children>
													</template>
												</children>
											</tablecol>
										</children>
									</tablerow>
									<tablerow>
										<children>
											<tablecol>
												<children>
													<text fixtext="ConsolType"/>
												</children>
											</tablecol>
											<tablecol dynamic="1">
												<children>
													<template>
														<match match="ConsolType"/>
														<children>
															<xpath allchildren="1"/>
														</children>
													</template>
												</children>
											</tablecol>
										</children>
									</tablerow>
									<tablerow>
										<children>
											<tablecol>
												<children>
													<text fixtext="MessageDate"/>
												</children>
											</tablecol>
											<tablecol dynamic="1">
												<children>
													<template>
														<match match="MessageDate"/>
														<children>
															<xpath allchildren="1"/>
														</children>
													</template>
												</children>
											</tablecol>
										</children>
									</tablerow>
									<tablerow>
										<children>
											<tablecol>
												<children>
													<text fixtext="MessageSource"/>
												</children>
											</tablecol>
											<tablecol dynamic="1">
												<children>
													<template>
														<match match="MessageSource"/>
														<children>
															<table dynamic="1" topdown="0">
																<properties border="1"/>
																<children>
																	<tablebody>
																		<children>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="OrgCode"/>
																						</children>
																					</tablecol>
																					<tablecol dynamic="1">
																						<children>
																							<template>
																								<match match="@OrgCode"/>
																								<children>
																									<xpath allchildren="1"/>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="OrganisationDetails"/>
																						</children>
																					</tablecol>
																					<tablecol dynamic="1">
																						<children>
																							<template>
																								<match match="OrganisationDetails"/>
																								<children>
																									<table dynamic="1" topdown="0">
																										<properties border="1"/>
																										<children>
																											<tablebody>
																												<children>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Name"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Name"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Location"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Location"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="OrgAddress"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="OrgAddress"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="OrgTelephone"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="OrgTelephone"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Email"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Email"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="WebAddress"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="WebAddress"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="BusinessRegistrationNumber"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="BusinessRegistrationNumber"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="LocalAccountCode"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="LocalAccountCode"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																												</children>
																											</tablebody>
																										</children>
																									</table>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																		</children>
																	</tablebody>
																</children>
															</table>
														</children>
													</template>
												</children>
											</tablecol>
										</children>
									</tablerow>
									<tablerow>
										<children>
											<tablecol>
												<children>
													<text fixtext="MessageTarget"/>
												</children>
											</tablecol>
											<tablecol dynamic="1">
												<children>
													<template>
														<match match="MessageTarget"/>
														<children>
															<table dynamic="1" topdown="0">
																<properties border="1"/>
																<children>
																	<tablebody>
																		<children>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="OrgCode"/>
																						</children>
																					</tablecol>
																					<tablecol dynamic="1">
																						<children>
																							<template>
																								<match match="@OrgCode"/>
																								<children>
																									<xpath allchildren="1"/>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="OrganisationDetails"/>
																						</children>
																					</tablecol>
																					<tablecol dynamic="1">
																						<children>
																							<template>
																								<match match="OrganisationDetails"/>
																								<children>
																									<table dynamic="1" topdown="0">
																										<properties border="1"/>
																										<children>
																											<tablebody>
																												<children>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Name"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Name"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Location"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Location"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="OrgAddress"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="OrgAddress"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="OrgTelephone"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="OrgTelephone"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Email"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Email"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="WebAddress"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="WebAddress"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="BusinessRegistrationNumber"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="BusinessRegistrationNumber"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="LocalAccountCode"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="LocalAccountCode"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																												</children>
																											</tablebody>
																										</children>
																									</table>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																		</children>
																	</tablebody>
																</children>
															</table>
														</children>
													</template>
												</children>
											</tablecol>
										</children>
									</tablerow>
									<tablerow>
										<children>
											<tablecol>
												<children>
													<text fixtext="TransportMode"/>
												</children>
											</tablecol>
											<tablecol dynamic="1">
												<children>
													<template>
														<match match="TransportMode"/>
														<children>
															<select ownvalue="1" enumeration="1">
																<properties size="0"/>
															</select>
														</children>
													</template>
												</children>
											</tablecol>
										</children>
									</tablerow>
									<tablerow>
										<children>
											<tablecol>
												<children>
													<text fixtext="Containers"/>
												</children>
											</tablecol>
											<tablecol dynamic="1">
												<children>
													<template>
														<match match="Containers"/>
														<children>
															<table dynamic="1">
																<properties border="1"/>
																<children>
																	<tableheader>
																		<children>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="ContainerNumber"/>
																						</children>
																					</tablecol>
																					<tablecol>
																						<children>
																							<text fixtext="ContainerType"/>
																						</children>
																					</tablecol>
																					<tablecol>
																						<children>
																							<text fixtext="Seal"/>
																						</children>
																					</tablecol>
																					<tablecol>
																						<children>
																							<text fixtext="PackingMode"/>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																		</children>
																	</tableheader>
																	<tablebody>
																		<children>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<template>
																								<match match="ContainerNumber"/>
																								<children>
																									<xpath allchildren="1"/>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																					<tablecol>
																						<children>
																							<template>
																								<match match="ContainerType"/>
																								<children>
																									<xpath allchildren="1"/>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																					<tablecol>
																						<children>
																							<template>
																								<match match="Seal"/>
																								<children>
																									<xpath allchildren="1"/>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																					<tablecol>
																						<children>
																							<template>
																								<match match="PackingMode"/>
																								<children>
																									<xpath allchildren="1"/>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																		</children>
																	</tablebody>
																</children>
															</table>
														</children>
													</template>
												</children>
											</tablecol>
										</children>
									</tablerow>
									<tablerow>
										<children>
											<tablecol>
												<children>
													<text fixtext="Carrier"/>
												</children>
											</tablecol>
											<tablecol dynamic="1">
												<children>
													<template>
														<match match="Carrier"/>
														<children>
															<table dynamic="1" topdown="0">
																<properties border="1"/>
																<children>
																	<tablebody>
																		<children>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="OrgCode"/>
																						</children>
																					</tablecol>
																					<tablecol dynamic="1">
																						<children>
																							<template>
																								<match match="@OrgCode"/>
																								<children>
																									<xpath allchildren="1"/>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="OrganisationDetails"/>
																						</children>
																					</tablecol>
																					<tablecol dynamic="1">
																						<children>
																							<template>
																								<match match="OrganisationDetails"/>
																								<children>
																									<table dynamic="1" topdown="0">
																										<properties border="1"/>
																										<children>
																											<tablebody>
																												<children>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Name"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Name"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Location"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Location"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="OrgAddress"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="OrgAddress"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="OrgTelephone"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="OrgTelephone"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Email"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Email"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="WebAddress"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="WebAddress"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="BusinessRegistrationNumber"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="BusinessRegistrationNumber"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="LocalAccountCode"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="LocalAccountCode"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																												</children>
																											</tablebody>
																										</children>
																									</table>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																		</children>
																	</tablebody>
																</children>
															</table>
														</children>
													</template>
												</children>
											</tablecol>
										</children>
									</tablerow>
									<tablerow>
										<children>
											<tablecol>
												<children>
													<text fixtext="AgentReference"/>
												</children>
											</tablecol>
											<tablecol dynamic="1">
												<children>
													<template>
														<match match="AgentReference"/>
														<children>
															<xpath allchildren="1"/>
														</children>
													</template>
												</children>
											</tablecol>
										</children>
									</tablerow>
									<tablerow>
										<children>
											<tablecol>
												<children>
													<text fixtext="ImportForwarder"/>
												</children>
											</tablecol>
											<tablecol dynamic="1">
												<children>
													<template>
														<match match="ImportForwarder"/>
														<children>
															<table dynamic="1" topdown="0">
																<properties border="1"/>
																<children>
																	<tablebody>
																		<children>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="OrgCode"/>
																						</children>
																					</tablecol>
																					<tablecol dynamic="1">
																						<children>
																							<template>
																								<match match="@OrgCode"/>
																								<children>
																									<xpath allchildren="1"/>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="OrganisationDetails"/>
																						</children>
																					</tablecol>
																					<tablecol dynamic="1">
																						<children>
																							<template>
																								<match match="OrganisationDetails"/>
																								<children>
																									<table dynamic="1" topdown="0">
																										<properties border="1"/>
																										<children>
																											<tablebody>
																												<children>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Name"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Name"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Location"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Location"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="OrgAddress"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="OrgAddress"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="OrgTelephone"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="OrgTelephone"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Email"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Email"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="WebAddress"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="WebAddress"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="BusinessRegistrationNumber"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="BusinessRegistrationNumber"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="LocalAccountCode"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="LocalAccountCode"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																												</children>
																											</tablebody>
																										</children>
																									</table>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																		</children>
																	</tablebody>
																</children>
															</table>
														</children>
													</template>
												</children>
											</tablecol>
										</children>
									</tablerow>
									<tablerow>
										<children>
											<tablecol>
												<children>
													<text fixtext="ExportForwarder"/>
												</children>
											</tablecol>
											<tablecol dynamic="1">
												<children>
													<template>
														<match match="ExportForwarder"/>
														<children>
															<table dynamic="1" topdown="0">
																<properties border="1"/>
																<children>
																	<tablebody>
																		<children>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="OrgCode"/>
																						</children>
																					</tablecol>
																					<tablecol dynamic="1">
																						<children>
																							<template>
																								<match match="@OrgCode"/>
																								<children>
																									<xpath allchildren="1"/>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="OrganisationDetails"/>
																						</children>
																					</tablecol>
																					<tablecol dynamic="1">
																						<children>
																							<template>
																								<match match="OrganisationDetails"/>
																								<children>
																									<table dynamic="1" topdown="0">
																										<properties border="1"/>
																										<children>
																											<tablebody>
																												<children>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Name"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Name"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Location"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Location"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="OrgAddress"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="OrgAddress"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="OrgTelephone"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="OrgTelephone"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Email"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Email"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="WebAddress"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="WebAddress"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="BusinessRegistrationNumber"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="BusinessRegistrationNumber"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="LocalAccountCode"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="LocalAccountCode"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																												</children>
																											</tablebody>
																										</children>
																									</table>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																		</children>
																	</tablebody>
																</children>
															</table>
														</children>
													</template>
												</children>
											</tablecol>
										</children>
									</tablerow>
									<tablerow>
										<children>
											<tablecol>
												<children>
													<text fixtext="OriginCTO"/>
												</children>
											</tablecol>
											<tablecol dynamic="1">
												<children>
													<template>
														<match match="OriginCTO"/>
														<children>
															<table dynamic="1" topdown="0">
																<properties border="1"/>
																<children>
																	<tablebody>
																		<children>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="OrgCode"/>
																						</children>
																					</tablecol>
																					<tablecol dynamic="1">
																						<children>
																							<template>
																								<match match="@OrgCode"/>
																								<children>
																									<xpath allchildren="1"/>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="OrganisationDetails"/>
																						</children>
																					</tablecol>
																					<tablecol dynamic="1">
																						<children>
																							<template>
																								<match match="OrganisationDetails"/>
																								<children>
																									<table dynamic="1" topdown="0">
																										<properties border="1"/>
																										<children>
																											<tablebody>
																												<children>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Name"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Name"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Location"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Location"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="OrgAddress"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="OrgAddress"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="OrgTelephone"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="OrgTelephone"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Email"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Email"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="WebAddress"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="WebAddress"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="BusinessRegistrationNumber"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="BusinessRegistrationNumber"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="LocalAccountCode"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="LocalAccountCode"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																												</children>
																											</tablebody>
																										</children>
																									</table>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																		</children>
																	</tablebody>
																</children>
															</table>
														</children>
													</template>
												</children>
											</tablecol>
										</children>
									</tablerow>
									<tablerow>
										<children>
											<tablecol>
												<children>
													<text fixtext="DestinationCTO"/>
												</children>
											</tablecol>
											<tablecol dynamic="1">
												<children>
													<template>
														<match match="DestinationCTO"/>
														<children>
															<table dynamic="1" topdown="0">
																<properties border="1"/>
																<children>
																	<tablebody>
																		<children>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="OrgCode"/>
																						</children>
																					</tablecol>
																					<tablecol dynamic="1">
																						<children>
																							<template>
																								<match match="@OrgCode"/>
																								<children>
																									<xpath allchildren="1"/>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="OrganisationDetails"/>
																						</children>
																					</tablecol>
																					<tablecol dynamic="1">
																						<children>
																							<template>
																								<match match="OrganisationDetails"/>
																								<children>
																									<table dynamic="1" topdown="0">
																										<properties border="1"/>
																										<children>
																											<tablebody>
																												<children>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Name"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Name"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Location"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Location"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="OrgAddress"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="OrgAddress"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="OrgTelephone"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="OrgTelephone"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Email"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Email"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="WebAddress"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="WebAddress"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="BusinessRegistrationNumber"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="BusinessRegistrationNumber"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="LocalAccountCode"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="LocalAccountCode"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																												</children>
																											</tablebody>
																										</children>
																									</table>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																		</children>
																	</tablebody>
																</children>
															</table>
														</children>
													</template>
												</children>
											</tablecol>
										</children>
									</tablerow>
									<tablerow>
										<children>
											<tablecol>
												<children>
													<text fixtext="OriginDepot"/>
												</children>
											</tablecol>
											<tablecol dynamic="1">
												<children>
													<template>
														<match match="OriginDepot"/>
														<children>
															<table dynamic="1" topdown="0">
																<properties border="1"/>
																<children>
																	<tablebody>
																		<children>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="OrgCode"/>
																						</children>
																					</tablecol>
																					<tablecol dynamic="1">
																						<children>
																							<template>
																								<match match="@OrgCode"/>
																								<children>
																									<xpath allchildren="1"/>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="OrganisationDetails"/>
																						</children>
																					</tablecol>
																					<tablecol dynamic="1">
																						<children>
																							<template>
																								<match match="OrganisationDetails"/>
																								<children>
																									<table dynamic="1" topdown="0">
																										<properties border="1"/>
																										<children>
																											<tablebody>
																												<children>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Name"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Name"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Location"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Location"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="OrgAddress"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="OrgAddress"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="OrgTelephone"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="OrgTelephone"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Email"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Email"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="WebAddress"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="WebAddress"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="BusinessRegistrationNumber"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="BusinessRegistrationNumber"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="LocalAccountCode"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="LocalAccountCode"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																												</children>
																											</tablebody>
																										</children>
																									</table>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																		</children>
																	</tablebody>
																</children>
															</table>
														</children>
													</template>
												</children>
											</tablecol>
										</children>
									</tablerow>
									<tablerow>
										<children>
											<tablecol>
												<children>
													<text fixtext="DestinationDepot"/>
												</children>
											</tablecol>
											<tablecol dynamic="1">
												<children>
													<template>
														<match match="DestinationDepot"/>
														<children>
															<xpath allchildren="1"/>
														</children>
													</template>
												</children>
											</tablecol>
										</children>
									</tablerow>
									<tablerow>
										<children>
											<tablecol>
												<children>
													<text fixtext="Leg"/>
												</children>
											</tablecol>
											<tablecol dynamic="1">
												<children>
													<template>
														<match match="Leg"/>
														<children>
															<table dynamic="1" topdown="0">
																<properties border="1"/>
																<children>
																	<tablebody>
																		<children>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="FlightNumber"/>
																						</children>
																					</tablecol>
																					<tablecol dynamic="1">
																						<children>
																							<template>
																								<match match="FlightNumber"/>
																								<children>
																									<xpath allchildren="1"/>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="FlightArrivalDate"/>
																						</children>
																					</tablecol>
																					<tablecol dynamic="1">
																						<children>
																							<template>
																								<match match="FlightArrivalDate"/>
																								<children>
																									<xpath allchildren="1"/>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="Vessel"/>
																						</children>
																					</tablecol>
																					<tablecol dynamic="1">
																						<children>
																							<template>
																								<match match="Vessel"/>
																								<children>
																									<xpath allchildren="1"/>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="VoyageNumber"/>
																						</children>
																					</tablecol>
																					<tablecol dynamic="1">
																						<children>
																							<template>
																								<match match="VoyageNumber"/>
																								<children>
																									<xpath allchildren="1"/>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="PortDetails"/>
																						</children>
																					</tablecol>
																					<tablecol dynamic="1">
																						<children>
																							<template>
																								<match match="PortDetails"/>
																								<children>
																									<table dynamic="1">
																										<properties border="1"/>
																										<children>
																											<tableheader>
																												<children>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="MovementType"/>
																																</children>
																															</tablecol>
																															<tablecol>
																																<children>
																																	<text fixtext="Port"/>
																																</children>
																															</tablecol>
																															<tablecol>
																																<children>
																																	<text fixtext="EstimatedDateTime"/>
																																</children>
																															</tablecol>
																															<tablecol>
																																<children>
																																	<text fixtext="ActualDateTime"/>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																												</children>
																											</tableheader>
																											<tablebody>
																												<children>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<template>
																																		<match match="@MovementType"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																															<tablecol>
																																<children>
																																	<template>
																																		<match match="Port"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																															<tablecol>
																																<children>
																																	<template>
																																		<match match="EstimatedDateTime"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																															<tablecol>
																																<children>
																																	<template>
																																		<match match="ActualDateTime"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																												</children>
																											</tablebody>
																										</children>
																									</table>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																		</children>
																	</tablebody>
																</children>
															</table>
														</children>
													</template>
												</children>
											</tablecol>
										</children>
									</tablerow>
									<tablerow>
										<children>
											<tablecol>
												<children>
													<text fixtext="Shipments"/>
												</children>
											</tablecol>
											<tablecol dynamic="1">
												<children>
													<template>
														<match match="Shipments"/>
														<children>
															<table dynamic="1" topdown="0">
																<properties border="1"/>
																<children>
																	<tablebody>
																		<children>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="HouseBill"/>
																						</children>
																					</tablecol>
																					<tablecol dynamic="1"/>
																				</children>
																			</tablerow>
																			<tablerow>
																				<children>
																					<tablecol>
																						<children>
																							<text fixtext="ShipmentDetail"/>
																						</children>
																					</tablecol>
																					<tablecol dynamic="1">
																						<children>
																							<template>
																								<match match="ShipmentDetail"/>
																								<children>
																									<table dynamic="1" topdown="0">
																										<properties border="1"/>
																										<children>
																											<tablebody>
																												<children>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="HouseBill"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="ShipmentIdentifiier"/>
																																		<children>
																																			<template>
																																				<match match="@ShipmentIdentifierType"/>
																																				<children>
																																					<select ownvalue="1" enumeration="1">
																																						<properties size="0"/>
																																					</select>
																																				</children>
																																			</template>
																																		</children>
																																	</template>
																																	<template>
																																		<match match="ShipmentIdentifiier"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="ShipmentStatus"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="ShipmentStatus"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="PortOfOrigin"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="PortOfOrigin"/>
																																		<children>
																																			<table dynamic="1" topdown="0">
																																				<properties border="1"/>
																																				<children>
																																					<tablebody>
																																						<children>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<text fixtext="MovementType"/>
																																										</children>
																																									</tablecol>
																																									<tablecol dynamic="1">
																																										<children>
																																											<template>
																																												<match match="@MovementType"/>
																																												<children>
																																													<select ownvalue="1" enumeration="1">
																																														<properties size="0"/>
																																													</select>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<text fixtext="Port"/>
																																										</children>
																																									</tablecol>
																																									<tablecol dynamic="1">
																																										<children>
																																											<template>
																																												<match match="Port"/>
																																												<children>
																																													<xpath allchildren="1"/>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<text fixtext="EstimatedDateTime"/>
																																										</children>
																																									</tablecol>
																																									<tablecol dynamic="1">
																																										<children>
																																											<template>
																																												<match match="EstimatedDateTime"/>
																																												<children>
																																													<xpath allchildren="1"/>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<text fixtext="ActualDateTime"/>
																																										</children>
																																									</tablecol>
																																									<tablecol dynamic="1">
																																										<children>
																																											<template>
																																												<match match="ActualDateTime"/>
																																												<children>
																																													<xpath allchildren="1"/>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																						</children>
																																					</tablebody>
																																				</children>
																																			</table>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="PortofDestination"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="PortofDestination"/>
																																		<children>
																																			<table dynamic="1" topdown="0">
																																				<properties border="1"/>
																																				<children>
																																					<tablebody>
																																						<children>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<text fixtext="MovementType"/>
																																										</children>
																																									</tablecol>
																																									<tablecol dynamic="1">
																																										<children>
																																											<template>
																																												<match match="@MovementType"/>
																																												<children>
																																													<select ownvalue="1" enumeration="1">
																																														<properties size="0"/>
																																													</select>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<text fixtext="Port"/>
																																										</children>
																																									</tablecol>
																																									<tablecol dynamic="1">
																																										<children>
																																											<template>
																																												<match match="Port"/>
																																												<children>
																																													<xpath allchildren="1"/>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<text fixtext="EstimatedDateTime"/>
																																										</children>
																																									</tablecol>
																																									<tablecol dynamic="1">
																																										<children>
																																											<template>
																																												<match match="EstimatedDateTime"/>
																																												<children>
																																													<xpath allchildren="1"/>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<text fixtext="ActualDateTime"/>
																																										</children>
																																									</tablecol>
																																									<tablecol dynamic="1">
																																										<children>
																																											<template>
																																												<match match="ActualDateTime"/>
																																												<children>
																																													<xpath allchildren="1"/>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																						</children>
																																					</tablebody>
																																				</children>
																																			</table>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Supplier"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Supplier"/>
																																		<children>
																																			<table dynamic="1" topdown="0">
																																				<properties border="1"/>
																																				<children>
																																					<tablebody>
																																						<children>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<text fixtext="OrgCode"/>
																																										</children>
																																									</tablecol>
																																									<tablecol dynamic="1">
																																										<children>
																																											<template>
																																												<match match="@OrgCode"/>
																																												<children>
																																													<xpath allchildren="1"/>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<text fixtext="OrganisationDetails"/>
																																										</children>
																																									</tablecol>
																																									<tablecol dynamic="1">
																																										<children>
																																											<template>
																																												<match match="OrganisationDetails"/>
																																												<children>
																																													<table dynamic="1" topdown="0">
																																														<properties border="1"/>
																																														<children>
																																															<tablebody>
																																																<children>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="Name"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="Name"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="Location"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="Location"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="OrgAddress"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="OrgAddress"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="OrgTelephone"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="OrgTelephone"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="Email"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="Email"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="WebAddress"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="WebAddress"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="BusinessRegistrationNumber"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="BusinessRegistrationNumber"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="LocalAccountCode"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="LocalAccountCode"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																</children>
																																															</tablebody>
																																														</children>
																																													</table>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																						</children>
																																					</tablebody>
																																				</children>
																																			</table>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="NotifyParty"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="NotifyParty"/>
																																		<children>
																																			<table dynamic="1" topdown="0">
																																				<properties border="1"/>
																																				<children>
																																					<tablebody>
																																						<children>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<text fixtext="OrgCode"/>
																																										</children>
																																									</tablecol>
																																									<tablecol dynamic="1">
																																										<children>
																																											<template>
																																												<match match="@OrgCode"/>
																																												<children>
																																													<xpath allchildren="1"/>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<text fixtext="OrganisationDetails"/>
																																										</children>
																																									</tablecol>
																																									<tablecol dynamic="1">
																																										<children>
																																											<template>
																																												<match match="OrganisationDetails"/>
																																												<children>
																																													<table dynamic="1" topdown="0">
																																														<properties border="1"/>
																																														<children>
																																															<tablebody>
																																																<children>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="Name"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="Name"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="Location"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="Location"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="OrgAddress"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="OrgAddress"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="OrgTelephone"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="OrgTelephone"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="Email"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="Email"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="WebAddress"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="WebAddress"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="BusinessRegistrationNumber"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="BusinessRegistrationNumber"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="LocalAccountCode"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="LocalAccountCode"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																</children>
																																															</tablebody>
																																														</children>
																																													</table>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																						</children>
																																					</tablebody>
																																				</children>
																																			</table>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Importer"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Importer"/>
																																		<children>
																																			<table dynamic="1" topdown="0">
																																				<properties border="1"/>
																																				<children>
																																					<tablebody>
																																						<children>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<text fixtext="OrgCode"/>
																																										</children>
																																									</tablecol>
																																									<tablecol dynamic="1">
																																										<children>
																																											<template>
																																												<match match="@OrgCode"/>
																																												<children>
																																													<xpath allchildren="1"/>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<text fixtext="OrganisationDetails"/>
																																										</children>
																																									</tablecol>
																																									<tablecol dynamic="1">
																																										<children>
																																											<template>
																																												<match match="OrganisationDetails"/>
																																												<children>
																																													<table dynamic="1" topdown="0">
																																														<properties border="1"/>
																																														<children>
																																															<tablebody>
																																																<children>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="Name"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="Name"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="Location"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="Location"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="OrgAddress"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="OrgAddress"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="OrgTelephone"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="OrgTelephone"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="Email"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="Email"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="WebAddress"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="WebAddress"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="BusinessRegistrationNumber"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="BusinessRegistrationNumber"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="LocalAccountCode"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol dynamic="1">
																																																				<children>
																																																					<template>
																																																						<match match="LocalAccountCode"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																</children>
																																															</tablebody>
																																														</children>
																																													</table>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																						</children>
																																					</tablebody>
																																				</children>
																																			</table>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Incoterm"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Incoterm"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="ShippingCharge"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="ShippingCharge"/>
																																		<children>
																																			<table dynamic="1">
																																				<properties border="1"/>
																																				<children>
																																					<tableheader>
																																						<children>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<text fixtext="ChargeDetails"/>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																						</children>
																																					</tableheader>
																																					<tablebody>
																																						<children>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<template>
																																												<match match="ChargeDetails"/>
																																												<children>
																																													<xpath allchildren="1"/>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																						</children>
																																					</tablebody>
																																				</children>
																																			</table>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="PaymentTerms"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="PaymentTerms"/>
																																		<children>
																																			<select ownvalue="1" enumeration="1">
																																				<properties size="0"/>
																																			</select>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="OrderNumber"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="OrderNumber"/>
																																		<children>
																																			<xpath allchildren="1"/>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Container"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Container"/>
																																		<children>
																																			<table dynamic="1">
																																				<properties border="1"/>
																																				<children>
																																					<tableheader>
																																						<children>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<text fixtext="ContainerNumber"/>
																																										</children>
																																									</tablecol>
																																									<tablecol>
																																										<children>
																																											<text fixtext="ContainerType"/>
																																										</children>
																																									</tablecol>
																																									<tablecol>
																																										<children>
																																											<text fixtext="Seal"/>
																																										</children>
																																									</tablecol>
																																									<tablecol>
																																										<children>
																																											<text fixtext="PackingMode"/>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																						</children>
																																					</tableheader>
																																					<tablebody>
																																						<children>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<template>
																																												<match match="ContainerNumber"/>
																																												<children>
																																													<xpath allchildren="1"/>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																									<tablecol>
																																										<children>
																																											<template>
																																												<match match="ContainerType"/>
																																												<children>
																																													<xpath allchildren="1"/>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																									<tablecol>
																																										<children>
																																											<template>
																																												<match match="Seal"/>
																																												<children>
																																													<xpath allchildren="1"/>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																									<tablecol>
																																										<children>
																																											<template>
																																												<match match="PackingMode"/>
																																												<children>
																																													<xpath allchildren="1"/>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																						</children>
																																					</tablebody>
																																				</children>
																																			</table>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="InvoiceHeader"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="InvoiceHeader"/>
																																		<children>
																																			<table dynamic="1" topdown="0">
																																				<properties border="1"/>
																																				<children>
																																					<tablebody>
																																						<children>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<text fixtext="InvoiceNumber"/>
																																										</children>
																																									</tablecol>
																																									<tablecol dynamic="1">
																																										<children>
																																											<template>
																																												<match match="InvoiceNumber"/>
																																												<children>
																																													<xpath allchildren="1"/>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<text fixtext="InvoiceAmount"/>
																																										</children>
																																									</tablecol>
																																									<tablecol dynamic="1">
																																										<children>
																																											<template>
																																												<match match="InvoiceAmount"/>
																																												<children>
																																													<xpath allchildren="1"/>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<text fixtext="Commission"/>
																																										</children>
																																									</tablecol>
																																									<tablecol dynamic="1">
																																										<children>
																																											<template>
																																												<match match="Commission"/>
																																												<children>
																																													<xpath allchildren="1"/>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<text fixtext="InvoiceDate"/>
																																										</children>
																																									</tablecol>
																																									<tablecol dynamic="1">
																																										<children>
																																											<template>
																																												<match match="InvoiceDate"/>
																																												<children>
																																													<xpath allchildren="1"/>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<text fixtext="InvoiceLine"/>
																																										</children>
																																									</tablecol>
																																									<tablecol dynamic="1">
																																										<children>
																																											<template>
																																												<match match="InvoiceLine"/>
																																												<children>
																																													<table dynamic="1">
																																														<properties border="1"/>
																																														<children>
																																															<tableheader>
																																																<children>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="LineNumber"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="PartNumber"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="Description"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="InvoiceQty"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="LinePrice"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="OrderNumber"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="OrderLineNumber"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="ClassificationHint"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="Volume"/>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol>
																																																				<children>
																																																					<text fixtext="Weight"/>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																</children>
																																															</tableheader>
																																															<tablebody>
																																																<children>
																																																	<tablerow>
																																																		<children>
																																																			<tablecol>
																																																				<children>
																																																					<template>
																																																						<match match="LineNumber"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol>
																																																				<children>
																																																					<template>
																																																						<match match="PartNumber"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol>
																																																				<children>
																																																					<template>
																																																						<match match="Description"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol>
																																																				<children>
																																																					<template>
																																																						<match match="InvoiceQty"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol>
																																																				<children>
																																																					<template>
																																																						<match match="LinePrice"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol>
																																																				<children>
																																																					<template>
																																																						<match match="OrderNumber"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol>
																																																				<children>
																																																					<template>
																																																						<match match="OrderLineNumber"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol>
																																																				<children>
																																																					<template>
																																																						<match match="ClassificationHint"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol>
																																																				<children>
																																																					<template>
																																																						<match match="Volume"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																			<tablecol>
																																																				<children>
																																																					<template>
																																																						<match match="Weight"/>
																																																						<children>
																																																							<xpath allchildren="1"/>
																																																						</children>
																																																					</template>
																																																				</children>
																																																			</tablecol>
																																																		</children>
																																																	</tablerow>
																																																</children>
																																															</tablebody>
																																														</children>
																																													</table>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																						</children>
																																					</tablebody>
																																				</children>
																																			</table>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																													<tablerow>
																														<children>
																															<tablecol>
																																<children>
																																	<text fixtext="Items"/>
																																</children>
																															</tablecol>
																															<tablecol dynamic="1">
																																<children>
																																	<template>
																																		<match match="Items"/>
																																		<children>
																																			<table dynamic="1">
																																				<properties border="1"/>
																																				<children>
																																					<tableheader>
																																						<children>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<text fixtext="ItemType"/>
																																										</children>
																																									</tablecol>
																																									<tablecol>
																																										<children>
																																											<text fixtext="GoodsValueOfItems"/>
																																										</children>
																																									</tablecol>
																																									<tablecol>
																																										<children>
																																											<text fixtext="NumberOfItemsOfThisType"/>
																																										</children>
																																									</tablecol>
																																									<tablecol>
																																										<children>
																																											<text fixtext="FirstItemNumber"/>
																																										</children>
																																									</tablecol>
																																									<tablecol>
																																										<children>
																																											<text fixtext="LastItemNumber"/>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																						</children>
																																					</tableheader>
																																					<tablebody>
																																						<children>
																																							<tablerow>
																																								<children>
																																									<tablecol>
																																										<children>
																																											<template>
																																												<match match="ItemType"/>
																																												<children>
																																													<xpath allchildren="1"/>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																									<tablecol>
																																										<children>
																																											<template>
																																												<match match="GoodsValueOfItems"/>
																																												<children>
																																													<xpath allchildren="1"/>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																									<tablecol>
																																										<children>
																																											<template>
																																												<match match="NumberOfItemsOfThisType"/>
																																												<children>
																																													<xpath allchildren="1"/>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																									<tablecol>
																																										<children>
																																											<template>
																																												<match match="FirstItemNumber"/>
																																												<children>
																																													<xpath allchildren="1"/>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																									<tablecol>
																																										<children>
																																											<template>
																																												<match match="LastItemNumber"/>
																																												<children>
																																													<xpath allchildren="1"/>
																																												</children>
																																											</template>
																																										</children>
																																									</tablecol>
																																								</children>
																																							</tablerow>
																																						</children>
																																					</tablebody>
																																				</children>
																																			</table>
																																		</children>
																																	</template>
																																</children>
																															</tablecol>
																														</children>
																													</tablerow>
																												</children>
																											</tablebody>
																										</children>
																									</table>
																								</children>
																							</template>
																						</children>
																					</tablecol>
																				</children>
																			</tablerow>
																		</children>
																	</tablebody>
																</children>
															</table>
														</children>
													</template>
												</children>
											</tablecol>
										</children>
									</tablerow>
								</children>
							</tablebody>
						</children>
					</table>
				</children>
			</template>
		</children>
	</template>
	<template>
		<match match="Consol"/>
		<children>
			<table dynamic="1" topdown="0">
				<properties border="1"/>
				<children>
					<tablebody>
						<children>
							<tablerow>
								<children>
									<tablecol>
										<children>
											<text fixtext="MasterWaybillNumber"/>
										</children>
									</tablecol>
									<tablecol dynamic="1">
										<children>
											<template>
												<match match="MasterWaybillNumber"/>
												<children>
													<xpath allchildren="1"/>
												</children>
											</template>
										</children>
									</tablecol>
								</children>
							</tablerow>
							<tablerow>
								<children>
									<tablecol>
										<children>
											<text fixtext="ConsolType"/>
										</children>
									</tablecol>
									<tablecol dynamic="1">
										<children>
											<template>
												<match match="ConsolType"/>
												<children>
													<xpath allchildren="1"/>
												</children>
											</template>
										</children>
									</tablecol>
								</children>
							</tablerow>
							<tablerow>
								<children>
									<tablecol>
										<children>
											<text fixtext="MessageDate"/>
										</children>
									</tablecol>
									<tablecol dynamic="1">
										<children>
											<template>
												<match match="MessageDate"/>
												<children>
													<table dynamic="1" topdown="0">
														<properties border="1"/>
														<children>
															<tablebody>
																<children>
																	<tablerow>
																		<children>
																			<tablecol>
																				<children>
																					<text fixtext="MasterWaybillNumber"/>
																				</children>
																			</tablecol>
																			<tablecol dynamic="1">
																				<children>
																					<template>
																						<match match="MasterWaybillNumber"/>
																						<children>
																							<xpath allchildren="1"/>
																						</children>
																					</template>
																				</children>
																			</tablecol>
																		</children>
																	</tablerow>
																	<tablerow>
																		<children>
																			<tablecol>
																				<children>
																					<text fixtext="ConsolType"/>
																				</children>
																			</tablecol>
																			<tablecol dynamic="1">
																				<children>
																					<template>
																						<match match="ConsolType"/>
																						<children>
																							<xpath allchildren="1"/>
																						</children>
																					</template>
																				</children>
																			</tablecol>
																		</children>
																	</tablerow>
																	<tablerow>
																		<children>
																			<tablecol>
																				<children>
																					<text fixtext="MessageDate"/>
																				</children>
																			</tablecol>
																			<tablecol dynamic="1">
																				<children>
																					<template>
																						<match match="MessageDate"/>
																						<children>
																							<xpath allchildren="1"/>
																						</children>
																					</template>
																				</children>
																			</tablecol>
																		</children>
																	</tablerow>
																	<tablerow>
																		<children>
																			<tablecol>
																				<children>
																					<text fixtext="MessageSource"/>
																				</children>
																			</tablecol>
																			<tablecol dynamic="1">
																				<children>
																					<template>
																						<match match="MessageSource"/>
																						<children>
																							<xpath allchildren="1"/>
																						</children>
																					</template>
																				</children>
																			</tablecol>
																		</children>
																	</tablerow>
																	<tablerow>
																		<children>
																			<tablecol>
																				<children>
																					<text fixtext="MessageTarget"/>
																				</children>
																			</tablecol>
																			<tablecol dynamic="1">
																				<children>
																					<template>
																						<match match="MessageTarget"/>
																						<children>
																							<xpath allchildren="1"/>
																						</children>
																					</template>
																				</children>
																			</tablecol>
																		</children>
																	</tablerow>
																	<tablerow>
																		<children>
																			<tablecol>
																				<children>
																					<text fixtext="TransportMode"/>
																				</children>
																			</tablecol>
																			<tablecol dynamic="1">
																				<children>
																					<template>
																						<match match="TransportMode"/>
																						<children>
																							<xpath allchildren="1"/>
																						</children>
																					</template>
																				</children>
																			</tablecol>
																		</children>
																	</tablerow>
																	<tablerow>
																		<children>
																			<tablecol>
																				<children>
																					<text fixtext="Containers"/>
																				</children>
																			</tablecol>
																			<tablecol dynamic="1">
																				<children>
																					<template>
																						<match match="Containers"/>
																						<children>
																							<xpath allchildren="1"/>
																						</children>
																					</template>
																				</children>
																			</tablecol>
																		</children>
																	</tablerow>
																	<tablerow>
																		<children>
																			<tablecol>
																				<children>
																					<text fixtext="Carrier"/>
																				</children>
																			</tablecol>
																			<tablecol dynamic="1">
																				<children>
																					<template>
																						<match match="Carrier"/>
																						<children>
																							<xpath allchildren="1"/>
																						</children>
																					</template>
																				</children>
																			</tablecol>
																		</children>
																	</tablerow>
																	<tablerow>
																		<children>
																			<tablecol>
																				<children>
																					<text fixtext="AgentReference"/>
																				</children>
																			</tablecol>
																			<tablecol dynamic="1">
																				<children>
																					<template>
																						<match match="AgentReference"/>
																						<children>
																							<xpath allchildren="1"/>
																						</children>
																					</template>
																				</children>
																			</tablecol>
																		</children>
																	</tablerow>
																	<tablerow>
																		<children>
																			<tablecol>
																				<children>
																					<text fixtext="ImportForwarder"/>
																				</children>
																			</tablecol>
																			<tablecol dynamic="1">
																				<children>
																					<template>
																						<match match="ImportForwarder"/>
																						<children>
																							<xpath allchildren="1"/>
																						</children>
																					</template>
																				</children>
																			</tablecol>
																		</children>
																	</tablerow>
																	<tablerow>
																		<children>
																			<tablecol>
																				<children>
																					<text fixtext="ExportForwarder"/>
																				</children>
																			</tablecol>
																			<tablecol dynamic="1">
																				<children>
																					<template>
																						<match match="ExportForwarder"/>
																						<children>
																							<xpath allchildren="1"/>
																						</children>
																					</template>
																				</children>
																			</tablecol>
																		</children>
																	</tablerow>
																	<tablerow>
																		<children>
																			<tablecol>
																				<children>
																					<text fixtext="OriginCTO"/>
																				</children>
																			</tablecol>
																			<tablecol dynamic="1">
																				<children>
																					<template>
																						<match match="OriginCTO"/>
																						<children>
																							<xpath allchildren="1"/>
																						</children>
																					</template>
																				</children>
																			</tablecol>
																		</children>
																	</tablerow>
																	<tablerow>
																		<children>
																			<tablecol>
																				<children>
																					<text fixtext="DestinationCTO"/>
																				</children>
																			</tablecol>
																			<tablecol dynamic="1">
																				<children>
																					<template>
																						<match match="DestinationCTO"/>
																						<children>
																							<xpath allchildren="1"/>
																						</children>
																					</template>
																				</children>
																			</tablecol>
																		</children>
																	</tablerow>
																	<tablerow>
																		<children>
																			<tablecol>
																				<children>
																					<text fixtext="OriginDepot"/>
																				</children>
																			</tablecol>
																			<tablecol dynamic="1">
																				<children>
																					<template>
																						<match match="OriginDepot"/>
																						<children>
																							<xpath allchildren="1"/>
																						</children>
																					</template>
																				</children>
																			</tablecol>
																		</children>
																	</tablerow>
																	<tablerow>
																		<children>
																			<tablecol>
																				<children>
																					<text fixtext="DestinationDepot"/>
																				</children>
																			</tablecol>
																			<tablecol dynamic="1">
																				<children>
																					<template>
																						<match match="DestinationDepot"/>
																						<children>
																							<xpath allchildren="1"/>
																						</children>
																					</template>
																				</children>
																			</tablecol>
																		</children>
																	</tablerow>
																	<tablerow>
																		<children>
																			<tablecol>
																				<children>
																					<text fixtext="Leg"/>
																				</children>
																			</tablecol>
																			<tablecol dynamic="1">
																				<children>
																					<template>
																						<match match="Leg"/>
																						<children>
																							<xpath allchildren="1"/>
																						</children>
																					</template>
																				</children>
																			</tablecol>
																		</children>
																	</tablerow>
																	<tablerow>
																		<children>
																			<tablecol>
																				<children>
																					<text fixtext="Shipments"/>
																				</children>
																			</tablecol>
																			<tablecol dynamic="1">
																				<children>
																					<template>
																						<match match="Shipments"/>
																						<children>
																							<xpath allchildren="1"/>
																						</children>
																					</template>
																				</children>
																			</tablecol>
																		</children>
																	</tablerow>
																</children>
															</tablebody>
														</children>
													</table>
												</children>
											</template>
										</children>
									</tablecol>
								</children>
							</tablerow>
							<tablerow>
								<children>
									<tablecol>
										<children>
											<text fixtext="MessageSource"/>
										</children>
									</tablecol>
									<tablecol dynamic="1">
										<children>
											<template>
												<match match="MessageSource"/>
												<children>
													<xpath allchildren="1"/>
												</children>
											</template>
										</children>
									</tablecol>
								</children>
							</tablerow>
							<tablerow>
								<children>
									<tablecol>
										<children>
											<text fixtext="MessageTarget"/>
										</children>
									</tablecol>
									<tablecol dynamic="1">
										<children>
											<template>
												<match match="MessageTarget"/>
												<children>
													<xpath allchildren="1"/>
												</children>
											</template>
										</children>
									</tablecol>
								</children>
							</tablerow>
							<tablerow>
								<children>
									<tablecol>
										<children>
											<text fixtext="TransportMode"/>
										</children>
									</tablecol>
									<tablecol dynamic="1">
										<children>
											<template>
												<match match="TransportMode"/>
												<children>
													<xpath allchildren="1"/>
												</children>
											</template>
										</children>
									</tablecol>
								</children>
							</tablerow>
							<tablerow>
								<children>
									<tablecol>
										<children>
											<text fixtext="Containers"/>
										</children>
									</tablecol>
									<tablecol dynamic="1">
										<children>
											<template>
												<match match="Containers"/>
												<children>
													<xpath allchildren="1"/>
												</children>
											</template>
										</children>
									</tablecol>
								</children>
							</tablerow>
							<tablerow>
								<children>
									<tablecol>
										<children>
											<text fixtext="Carrier"/>
										</children>
									</tablecol>
									<tablecol dynamic="1">
										<children>
											<template>
												<match match="Carrier"/>
												<children>
													<xpath allchildren="1"/>
												</children>
											</template>
										</children>
									</tablecol>
								</children>
							</tablerow>
							<tablerow>
								<children>
									<tablecol>
										<children>
											<text fixtext="AgentReference"/>
										</children>
									</tablecol>
									<tablecol dynamic="1">
										<children>
											<template>
												<match match="AgentReference"/>
												<children>
													<xpath allchildren="1"/>
												</children>
											</template>
										</children>
									</tablecol>
								</children>
							</tablerow>
							<tablerow>
								<children>
									<tablecol>
										<children>
											<text fixtext="ImportForwarder"/>
										</children>
									</tablecol>
									<tablecol dynamic="1">
										<children>
											<template>
												<match match="ImportForwarder"/>
												<children>
													<xpath allchildren="1"/>
												</children>
											</template>
										</children>
									</tablecol>
								</children>
							</tablerow>
							<tablerow>
								<children>
									<tablecol>
										<children>
											<text fixtext="ExportForwarder"/>
										</children>
									</tablecol>
									<tablecol dynamic="1">
										<children>
											<template>
												<match match="ExportForwarder"/>
												<children>
													<xpath allchildren="1"/>
												</children>
											</template>
										</children>
									</tablecol>
								</children>
							</tablerow>
							<tablerow>
								<children>
									<tablecol>
										<children>
											<text fixtext="OriginCTO"/>
										</children>
									</tablecol>
									<tablecol dynamic="1">
										<children>
											<template>
												<match match="OriginCTO"/>
												<children>
													<xpath allchildren="1"/>
												</children>
											</template>
										</children>
									</tablecol>
								</children>
							</tablerow>
							<tablerow>
								<children>
									<tablecol>
										<children>
											<text fixtext="DestinationCTO"/>
										</children>
									</tablecol>
									<tablecol dynamic="1">
										<children>
											<template>
												<match match="DestinationCTO"/>
												<children>
													<xpath allchildren="1"/>
												</children>
											</template>
										</children>
									</tablecol>
								</children>
							</tablerow>
							<tablerow>
								<children>
									<tablecol>
										<children>
											<text fixtext="OriginDepot"/>
										</children>
									</tablecol>
									<tablecol dynamic="1">
										<children>
											<template>
												<match match="OriginDepot"/>
												<children>
													<xpath allchildren="1"/>
												</children>
											</template>
										</children>
									</tablecol>
								</children>
							</tablerow>
							<tablerow>
								<children>
									<tablecol>
										<children>
											<text fixtext="DestinationDepot"/>
										</children>
									</tablecol>
									<tablecol dynamic="1">
										<children>
											<template>
												<match match="DestinationDepot"/>
												<children>
													<xpath allchildren="1"/>
												</children>
											</template>
										</children>
									</tablecol>
								</children>
							</tablerow>
							<tablerow>
								<children>
									<tablecol>
										<children>
											<text fixtext="Leg"/>
										</children>
									</tablecol>
									<tablecol dynamic="1">
										<children>
											<template>
												<match match="Leg"/>
												<children>
													<xpath allchildren="1"/>
												</children>
											</template>
										</children>
									</tablecol>
								</children>
							</tablerow>
							<tablerow>
								<children>
									<tablecol>
										<children>
											<text fixtext="Shipments"/>
										</children>
									</tablecol>
									<tablecol dynamic="1">
										<children>
											<template>
												<match match="Shipments"/>
												<children>
													<xpath allchildren="1"/>
												</children>
											</template>
										</children>
									</tablecol>
								</children>
							</tablerow>
						</children>
					</tablebody>
				</children>
			</table>
		</children>
	</template>
	<pagelayout>
		<properties pagemultiplepages="0" pagenumberingformat="1" pagenumberingstartat="1" paperheight="11in" papermarginbottom="0.79in" papermarginleft="0.6in" papermarginright="0.6in" papermargintop="0.79in" paperwidth="8.5in"/>
	</pagelayout>
</structure>
